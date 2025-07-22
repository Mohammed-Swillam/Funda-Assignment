using System.ComponentModel.DataAnnotations;

namespace FundaMakelaarsAnalyzer.Configuration;

public class ApiConfiguration
{
    public required string BaseUrl { get; set; }
    public required string ApiKey { get; set; }
    public int PageSize { get; set; }
    public int RequestDelay { get; set; }
    public int RequestTimeout { get; set; }
}