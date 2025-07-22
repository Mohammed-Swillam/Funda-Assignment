using FundaMakelaarsAnalyzer.Models;

namespace FundaMakelaarsAnalyzer.Services;

public class MakelaarAnalyzer : IMakelaarAnalyzer
{
    private readonly IFundaApiClient _apiClient;

    public MakelaarAnalyzer(IFundaApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    public async Task<List<MakelaarStatistic>> GetTopMakelaarsAsync(string searchCriteria, int NumberOfMaklaars = 10, CancellationToken cancellationToken = default)
    {
        
        var allListings = await _apiClient.GetAllListingsAsync(searchCriteria, cancellationToken);        
        var statistics = ProcessListings(allListings);
        
        return statistics;
    }

    private static List<MakelaarStatistic> ProcessListings(List<HouseListing> listings)
    {
        if (!listings.Any())
        {
            Console.WriteLine("[Analysis] No listings to process");
            return new List<MakelaarStatistic>();
        }

        // Group by makelaar and count listings
        var statistics = listings
            //.Where(listing => !string.IsNullOrWhiteSpace(listing.MakelaarNaam)) // Filter out empty entries
            .GroupBy(listing => new { listing.MakelaarId, listing.MakelaarNaam })
            .Select(listing => new MakelaarStatistic
            {
                MakelaarId = listing.Key.MakelaarId,
                MakelaarNaam = listing.Key.MakelaarNaam.Trim(),
                AantalObjecten = listing.Count()
            })
            .OrderByDescending(stat => stat.AantalObjecten)
            .Take(10)
            .ToList();

        return statistics;
    }
}