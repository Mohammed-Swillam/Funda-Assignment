namespace FundaMakelaarsAnalyzer.Models;

public class FundaApiResponse
{
    public int AccountStatus { get; set; }
    public bool EmailNotConfirmed { get; set; }
    public bool ValidationFailed { get; set; }
    public object? ValidationReport { get; set; }
    public int Website { get; set; }
    public Metadata Metadata { get; set; } = new();
    public List<HouseListing> Objects { get; set; } = new();
    public Paging Paging { get; set; } = new();
    public int TotaalAantalObjecten { get; set; }
}

public class HouseListing
{
    public int MakelaarId { get; set; }
    public string MakelaarNaam { get; set; } = "Empty Name";
    public string Id { get; set; } = string.Empty;
    public string Adres { get; set; } = string.Empty;
    public string Woonplaats { get; set; } = string.Empty;
    public int? Koopprijs { get; set; }
    public int? AantalKamers { get; set; }
    public int? Perceeloppervlakte { get; set; }
    public int? Woonoppervlakte { get; set; }
    public string Postcode { get; set; } = string.Empty;
}

public class Metadata
{
    public string ObjectType { get; set; } = string.Empty;
    public string Omschrijving { get; set; } = string.Empty;
    public string Titel { get; set; } = string.Empty;
}

public class Paging
{
    public int AantalPaginas { get; set; }
    public int HuidigePagina { get; set; }
    public string? VolgendeUrl { get; set; }
    public string? VorigeUrl { get; set; }
}

public class MakelaarStatistic
{
    public int MakelaarId { get; set; }
    public string MakelaarNaam { get; set; } = string.Empty;
    public int AantalObjecten { get; set; }
}