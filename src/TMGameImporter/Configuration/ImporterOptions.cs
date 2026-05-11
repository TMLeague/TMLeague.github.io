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
    public string? CfClearance { get; set; }
    public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36";
}