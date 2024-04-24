namespace TMGameImporter.Configuration;

internal class ImporterOptions
{
    public string BaseLocation { get; set; } = "../TMLeague/wwwroot/data";
    public bool FetchFinishedDivisions { get; set; }
    public bool FetchFinishedGames { get; set; }
    public string? League { get; set; }
    public string? Season { get; set; }
    public string[]? Seasons { get; set; }
    public string? Division { get; set; }
    public int[]? Games { get; set; }
}