using TMModels;

namespace TMApplication.ViewModels;

public record SummaryViewModel(
    string LeagueName,
    string DivisionName,
    IReadOnlyCollection<PlayerScoreViewModel> Players,
    IdName[] AvailableDivisions);

public record PlayerScoreViewModel(
    string Player,
    IReadOnlyDictionary<ScoreType, ScoreViewModel> Scores,
    int Seasons)
{
    public double TotalPoints(ScoreType type, int doubles = 1) => Math.Round(Scores[type].TotalPoints, doubles);
    public double Wins(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Wins, doubles);
    public double Cla(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Cla, doubles);
    public double Supplies(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Supplies, doubles);
    public double PowerTokens(ScoreType type, int doubles = 1) => Math.Round(Scores[type].PowerTokens, doubles);
    public double MinutesPerMove(ScoreType type, int doubles = 0) => Math.Round(Scores[type].MinutesPerMove ?? 0, doubles);
    public double Moves(ScoreType type, int doubles = 0) => Math.Round(Scores[type].Moves, doubles);
    public double PenaltiesPoints(ScoreType type, int doubles = 1) => Math.Round(Scores[type].PenaltiesPoints, doubles);
    public double Position(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Position ?? 0, doubles);
    public double Baratheon(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Baratheon, out var score) ? score : 0, doubles);
    public double Lannister(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Lannister, out var score) ? score : 0, doubles);
    public double Stark(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Stark, out var score) ? score : 0, doubles);
    public double Tyrell(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Tyrell, out var score) ? score : 0, doubles);
    public double Greyjoy(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Greyjoy, out var score) ? score : 0, doubles);
    public double Martell(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Martell, out var score) ? score : 0, doubles);
    public double Arryn(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Houses.TryGetValue(House.Arryn, out var score) ? score : 0, doubles);
}

public enum ScoreType
{
    Best, Average, Total
}

public static class ScoreTypes
{
    public static readonly ScoreType[] All = { ScoreType.Best, ScoreType.Average, ScoreType.Total };
}

public record ScoreViewModel(
    double TotalPoints,
    double Wins,
    double Cla,
    double Supplies,
    double PowerTokens,
    double? MinutesPerMove,
    double Moves,
    Dictionary<House, double> Houses,
    double PenaltiesPoints,
    double? Position);