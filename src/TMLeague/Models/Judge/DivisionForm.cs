using TMModels;

namespace TMLeague.Models.Judge;

public class DivisionForm
{
    public string Season { get; set; }
    public string Division { get; set; }
    public string Password { get; set; }
    public string Contact { get; set; }
    public string JudgeName { get; set; }
    public string SpecialNote { get; set; }
    public string Player1 { get; set; }
    public string Player2 { get; set; }
    public string Player3 { get; set; }
    public string Player4 { get; set; }
    public string Player5 { get; set; }
    public string Player6 { get; set; }
    public string Player7 { get; set; }
    public string Player8 { get; set; }
    public string Player9 { get; set; }
    public string Player10 { get; set; }
}

public record DivisionDraft(List<PlayerDraft> Draw);

public record PlayerDraft(string Name, House[] Games, string Message);