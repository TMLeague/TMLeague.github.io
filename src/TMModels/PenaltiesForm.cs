namespace TMModels;

public class PenaltiesForm
{
    public PenaltiesForm(string name, string judge, List<string> players, List<int?> games, List<PenaltyForm> penalties, List<ReplacementForm> replacements, bool isFinished, string? winnerTitle, int? promotions, int? relegations)
    {
        Name = name;
        Judge = judge;
        Players = players;
        Games = games;
        Penalties = penalties;
        Replacements = replacements;
        IsFinished = isFinished;
        WinnerTitle = winnerTitle;
        Promotions = promotions;
        Relegations = relegations;
    }

    public string Name { get; set; }
    public string Judge { get; set; }
    public List<string> Players { get; set; }
    public List<int?> Games { get; set; }
    public List<PenaltyForm> Penalties { get; set; }
    public List<ReplacementForm> Replacements { get; set; }
    public bool IsFinished { get; set; }
    public string? WinnerTitle { get; set; }
    public int? Promotions { get; set; }
    public int? Relegations { get; set; }
}

public class PenaltyForm
{
    public PenaltyForm(string player, int? game, double points, string details)
    {
        Player = player;
        Game = game;
        Points = points;
        Details = details;
    }

    public string Player { get; set; }
    public int? Game { get; set; }
    public double Points { get; set; }
    public string Details { get; set; }
}

public class ReplacementForm
{
    public ReplacementForm(string from, string to, int game)
    {
        From = from;
        To = to;
        Game = game;
    }

    public string From { get; set; }
    public string To { get; set; }
    public int Game { get; set; }
}