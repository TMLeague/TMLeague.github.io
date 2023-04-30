namespace TMDraftsGenerator;

internal class DraftOptions
{
    public int Players { get; set; } = 3;
    public int Houses { get; set; } = 6;
    public int Threads { get; set; } = 1;
    public string ResultsPath { get; set; } = "results";
}