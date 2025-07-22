using FundaMakelaarsAnalyzer.Models;

namespace FundaMakelaarsAnalyzer.Services;

public interface IFundaApiClient
{
    Task<FundaApiResponse> GetListingsAsync(string searchQuery, int page, CancellationToken cancellationToken = default);
    Task<List<HouseListing>> GetAllListingsAsync(string searchQuery, CancellationToken cancellationToken = default);
}

public interface IMakelaarAnalyzer
{
    Task<List<MakelaarStatistic>> GetTopMakelaarsAsync(string searchCriteria, int NumberOfMaklaars,CancellationToken cancellationToken = default);
}

