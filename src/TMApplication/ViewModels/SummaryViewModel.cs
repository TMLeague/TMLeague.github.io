using TMModels;

namespace TMApplication.ViewModels;

public record SummaryViewModel(
    string LeagueName,
    IReadOnlyCollection<SummaryDivisionViewModel> Divisions);

public record SummaryDivisionViewModel(
    string DivisionId,
    string DivisionName,
    IReadOnlyCollection<PlayerScoreViewModel> Players);

public record PlayerScoreViewModel(
    string Player,
    IReadOnlyDictionary<ScoreType, ScoreViewModel> Scores,
    uint Seasons)
{
    public double TotalPoints(ScoreType type, int decimals = 1) => Math.Round(Scores[type].TotalPoints, decimals);
    public double Wins(ScoreType type, int decimals = 1) => Math.Round(Scores[type].Wins, decimals);
    public double Cla(ScoreType type, int decimals = 1) => Math.Round(Scores[type].Cla, decimals);
    public double Supplies(ScoreType type, int decimals = 1) => Math.Round(Scores[type].Supplies, decimals);
    public double PowerTokens(ScoreType type, int decimals = 1) => Math.Round(Scores[type].PowerTokens, decimals);
    public double MinutesPerMove(ScoreType type, int decimals = 0) => Math.Round(Scores[type].MinutesPerMove, decimals);
    public double Moves(ScoreType type, int decimals = 0) => Math.Round(Scores[type].Moves, decimals);
    public double PenaltiesPoints(ScoreType type, int decimals = 1) => Math.Round(Scores[type].PenaltiesPoints, decimals);
    public double Baratheon(ScoreType type, int decimals = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Baratheon, out var score) ? score : 0, decimals);
    public double Lannister(ScoreType type, int decimals = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Lannister, out var score) ? score : 0, decimals);
    public double Stark(ScoreType type, int decimals = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Stark, out var score) ? score : 0, decimals);
    public double Tyrell(ScoreType type, int decimals = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Tyrell, out var score) ? score : 0, decimals);
    public double Greyjoy(ScoreType type, int decimals = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Greyjoy, out var score) ? score : 0, decimals);
    public double Martell(ScoreType type, int decimals = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Martell, out var score) ? score : 0, decimals);
    public double Arryn(ScoreType type, int decimals = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Arryn, out var score) ? score : 0, decimals);
}

public enum ScoreType
{
    Best, Average, Total
}

public record ScoreViewModel(
    double TotalPoints,
    double Wins,
    double Cla,
    double Supplies,
    double PowerTokens,
    double MinutesPerMove,
    double Moves,
    Dictionary<House, double> Houses,
    double PenaltiesPoints);