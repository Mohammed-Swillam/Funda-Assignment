using FundaMakelaarsAnalyzer.Models;

namespace FundaMakelaarsAnalyzer.Services;

public static class TableFormatter
{
    public static void PrintMakelaarStatistics(string title, List<MakelaarStatistic> statistics)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
        Console.WriteLine();
        
        if (!statistics.Any())
        {
            Console.WriteLine("No data available.");
            return;
        }

        Console.WriteLine($"{"Rank",-6} {"Makelaar Name",-50} {"# of Properties",-15}");
        Console.WriteLine(new string('-', 80));
        
        for (int i = 0; i < statistics.Count; i++)
        {
            var stat = statistics[i];
            Console.WriteLine($"{i + 1,-6} {stat.MakelaarNaam,-50} {stat.AantalObjecten,-15}");
        }
        
        Console.WriteLine();
    }

    public static void PrintProgressUpdate(string message, int current = 0, int total = 0)
    {
        if (total > 0)
        {
            var percentage = (double)current / total * 100;
            Console.WriteLine($"{message} ({current}/{total} - {percentage:F1}%)");
        }
        else
        {
            Console.WriteLine(message);
        }
    }
}