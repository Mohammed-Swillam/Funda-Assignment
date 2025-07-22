using System.Text.Json;
using FundaMakelaarsAnalyzer.Configuration;
using FundaMakelaarsAnalyzer.Models;
using Polly;

namespace FundaMakelaarsAnalyzer.Services;

public class FundaApiClient : IFundaApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ApiConfiguration _config;
    private readonly ResiliencePipeline _resiliencePipeline;
    private DateTime _lastRequestTime = DateTime.MinValue;

    public FundaApiClient(HttpClient httpClient, ApiConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;

        // Configure Polly 
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                MaxRetryAttempts = 10,
                Delay = TimeSpan.FromSeconds(10),
                BackoffType = Polly.DelayBackoffType.Constant,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>(),
                OnRetry = args =>
                {
                    Console.WriteLine($"[Retry] Attempt ({args.AttemptNumber} after {args.RetryDelay.TotalSeconds:F1}s - {args.Outcome.Exception?.Message ?? "HTTP error"}");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        // Configure HttpClient timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.RequestTimeout);
    }

    public async Task<FundaApiResponse> GetListingsAsync(string searchQuery, int page = 1, CancellationToken cancellationToken = default)
    {
        await EnforceRateLimit(cancellationToken);

        var url = BuildApiUrl(searchQuery, page);

        Console.WriteLine($"[API] Requesting page {page} for query '{searchQuery}'");

        HttpResponseMessage response;

        try
        {
            response = await _resiliencePipeline.ExecuteAsync(async token =>
            {
                var httpResponse = await _httpClient.GetAsync(url, token);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync(token);
                    throw new HttpRequestException(
                        $"API request failed with status {httpResponse.StatusCode}: {httpResponse.ReasonPhrase}. Content: {errorContent}");
                }

                return httpResponse;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] Request failed after retries: {ex.Message}");
            throw;
        }

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

        var apiResponse = JsonSerializer.Deserialize<FundaApiResponse>(jsonContent);

        if (apiResponse == null)
        {
            throw new InvalidOperationException("Failed to deserialize API response");
        }

        Console.WriteLine($"[API] Retrieved {apiResponse.Objects.Count} objects from page {page} (Total: {apiResponse.TotaalAantalObjecten})");

        return apiResponse;
    }

    public async Task<List<HouseListing>> GetAllListingsAsync(string searchQuery, CancellationToken cancellationToken = default)
    {
        var allListings = new List<HouseListing>();
        var currentPage = 1;

        Console.WriteLine($"[API] Starting to fetch all listings for: {searchQuery}");

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await GetListingsAsync(searchQuery, currentPage, cancellationToken);

            if (response.Objects?.Any() != true)
            {
                Console.WriteLine($"[API] No more objects found on page {currentPage}");
                break;
            }

            allListings.AddRange(response.Objects);

            TableFormatter.PrintProgressUpdate(
                $"[Progress] Fetching data",
                allListings.Count,
                response.TotaalAantalObjecten);

            // Check if we've reached the last page
            if (currentPage >= response.Paging.AantalPaginas)
            {
                Console.WriteLine($"[API] Reached last page ({currentPage} of {response.Paging.AantalPaginas})");
                break;
            }

            currentPage++;
        }

        Console.WriteLine($"[API] Completed fetching {allListings.Count} total listings");
        return allListings;
    }

    private async Task EnforceRateLimit(CancellationToken cancellationToken)
    {
        var timeSinceLastRequest = DateTime.Now - _lastRequestTime;
        if (timeSinceLastRequest < TimeSpan.FromMilliseconds(_config.RequestDelay))
        {
            var delayTime = TimeSpan.FromMilliseconds(_config.RequestDelay) - timeSinceLastRequest;
            Console.WriteLine($"[RateLimit] Waiting {delayTime.TotalMilliseconds:F0}ms to respect rate limit");
            await Task.Delay(delayTime, cancellationToken);
        }
        _lastRequestTime = DateTime.Now;
    }

    private string BuildApiUrl(string searchQuery, int page)
    {
        // Ensure search query starts and ends with a forward slash
        if (!searchQuery.StartsWith('/'))
        {
            searchQuery = "/" + searchQuery;
        }

        if (!searchQuery.EndsWith('/'))
        {
            searchQuery += "/";
        }

        return $"{_config.BaseUrl}/{_config.ApiKey}/?type=koop&zo={searchQuery}&page={page}&pagesize={_config.PageSize}";
    }
}