using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FundaMakelaarsAnalyzer.Services;
using FundaMakelaarsAnalyzer.Configuration;
using FundaMakelaarsAnalyzer.Models;
using Microsoft.Extensions.Configuration;

namespace FundaMakelaarsAnalyzer;

class FundaMakelaarsApp
{
    static async Task Main(string[] args)
    {
        // Create the dependency injection container
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Configuration
                var apiConfig = new ApiConfiguration();
                context.Configuration.GetSection("ApiConfiguration").Bind(apiConfig);
                services.AddSingleton(apiConfig);
                
                // Business Services
                services.AddScoped<IMakelaarAnalyzer, MakelaarAnalyzer>();
                
                // HTTP Client with proper configuration
                services.AddHttpClient<IFundaApiClient, FundaApiClient>();
            })
            .Build();

        try
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("           Funda Makelaars Analyzer");
            Console.WriteLine("==================================================");
            Console.WriteLine();
            
            var analyzer = host.Services.GetRequiredService<IMakelaarAnalyzer>();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(15)); // 15 minute timeout
            var cancellationToken = cancellationTokenSource.Token;


            // Analyze all Amsterdam properties
            Console.WriteLine("ANALYZING ALL AMSTERDAM PROPERTIES");
            Console.WriteLine("=====================================");
            var allAmsterdamStats = await analyzer.GetTopMakelaarsAsync("/amsterdam/", 10, cancellationToken);
            

            // Analyze Amsterdam properties with garden
            Console.WriteLine("ANALYZING AMSTERDAM PROPERTIES WITH GARDEN");
            Console.WriteLine("=============================================");
            var gardenStats = await analyzer.GetTopMakelaarsAsync("/amsterdam/tuin/", 10, cancellationToken);

            TableFormatter.PrintMakelaarStatistics("Top 10 Makelaars in Amsterdam: All Properties", allAmsterdamStats);
            TableFormatter.PrintMakelaarStatistics("Top 10 Makelaars in Amsterdam: Properties with Garden", gardenStats);

            Console.WriteLine("Analysis completed successfully!");
            Console.WriteLine();
            
            // Summary statistics
            PrintSummary(allAmsterdamStats, gardenStats);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was cancelled due to timeout.");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network error occurred: {ex.Message}");
            Console.WriteLine("Please check your internet connection and try again.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            Console.WriteLine($"Details: {ex}");
        }
        
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void PrintSummary(List<MakelaarStatistic> allStats, List<MakelaarStatistic> gardenStats)
    {
        Console.WriteLine(" SUMMARY");
        Console.WriteLine("===================");
        
        if (allStats.Any())
        {
            var totalAllProperties = allStats.Sum(s => s.AantalObjecten);
            var topMakelaarAll = allStats.First();
            
            Console.WriteLine($"Top makelaar (all Properties): {topMakelaarAll.MakelaarNaam} ({topMakelaarAll.AantalObjecten:N0} properties)");
        }
        
        if (gardenStats.Any())
        {
            var totalGardenProperties = gardenStats.Sum(s => s.AantalObjecten);
            var topMakelaarGarden = gardenStats.First();
            
            Console.WriteLine($"Top makelaar (Properties With garden): {topMakelaarGarden.MakelaarNaam} ({topMakelaarGarden.AantalObjecten:N0} properties)");
        }
        
        Console.WriteLine();
    }
}