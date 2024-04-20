using TMModels;

namespace TMApplication.ViewModels;

public record SummaryViewModel(
    string LeagueName,
    string DivisionName,
    IReadOnlyCollection<PlayerScoreViewModel> Players,
    IReadOnlyCollection<HouseScoreViewModel> Houses,
    IdName[] AvailableDivisions);

public record PlayerScoreViewModel(
    string PlayerName,
    IReadOnlyDictionary<ScoreType, PlayerScoreDetailsViewModel> Scores,
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
    public double Baratheon(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Baratheon, doubles);
    public double Lannister(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Lannister, doubles);
    public double Stark(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Stark, doubles);
    public double Tyrell(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Tyrell, doubles);
    public double Greyjoy(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Greyjoy, doubles);
    public double Martell(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Martell, doubles);
    public double Arryn(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Arryn, doubles);
    public BattleStats Battles(ScoreType type, int doubles = 1) => Scores[type].Stats.Battles;
    public UnitStats Kills(ScoreType type, int doubles = 1) => Scores[type].Stats.Kills;
    public UnitStats Casualties(ScoreType type, int doubles = 1) => Scores[type].Stats.Casualties;
    public PowerTokenStats PowerTokensGathered(ScoreType type, int doubles = 1) => Scores[type].Stats.PowerTokens;
    public BidStats PowerTokensSpent(ScoreType type, int doubles = 1) => Scores[type].Stats.Bids;
}

public record HouseScoreViewModel(
    House House,
    IReadOnlyDictionary<ScoreType, HouseScoreDetailsViewModel> Scores,
    int Games)
{
    public double TotalPoints(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Points, doubles);
    public double Wins(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Wins, doubles);
    public double Cla(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Cla, doubles);
    public double Supplies(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Supplies, doubles);
    public double PowerTokens(ScoreType type, int doubles = 1) => Math.Round(Scores[type].PowerTokens, doubles);
    public double Moves(ScoreType type, int doubles = 1) => Math.Round(Scores[type].Moves, doubles);
    public BattleStats Battles(ScoreType type, int doubles = 1) => Scores[type].Stats.Battles;
    public UnitStats Kills(ScoreType type, int doubles = 1) => Scores[type].Stats.Kills;
    public UnitStats Casualties(ScoreType type, int doubles = 1) => Scores[type].Stats.Casualties;
    public PowerTokenStats PowerTokensGathered(ScoreType type, int doubles = 1) => Scores[type].Stats.PowerTokens;
    public BidStats PowerTokensSpent(ScoreType type, int doubles = 1) => Scores[type].Stats.Bids;
}

public enum ScoreType
{
    Best, Average, Total
}

public static class ScoreTypes
{
    public static ScoreType[] All(TableType tableType) => tableType switch
    {
        TableType.Houses => new[] { ScoreType.Best, ScoreType.Average },
        _ => new[] { ScoreType.Best, ScoreType.Average, ScoreType.Total }
    };

    public static ScoreType? Get(TableType? tableType, ScoreType? scoreType) => tableType switch
    {
        TableType.Houses => scoreType switch
        {
            ScoreType.Total => ScoreType.Average,
            _ => scoreType
        },
        _ => scoreType
    };
}

public enum TableType
{
    Players, Houses
}

public static class TableTypes
{
    public static readonly TableType[] All = { TableType.Players, TableType.Houses };
}

public record PlayerScoreDetailsViewModel(
    double TotalPoints,
    double Wins,
    double Cla,
    double Supplies,
    double PowerTokens,
    double? MinutesPerMove,
    double Moves,
    Dictionary<House, double> Houses,
    double PenaltiesPoints,
    double? Position,
    Stats? Stats)
{
    public Stats Stats { get; } = Stats ?? new Stats(Houses.Keys);
    public double Baratheon => Houses.TryGetValue(House.Baratheon, out var score) ? score : 0;
    public double Lannister => Houses.TryGetValue(House.Lannister, out var score) ? score : 0;
    public double Stark => Houses.TryGetValue(House.Stark, out var score) ? score : 0;
    public double Tyrell => Houses.TryGetValue(House.Tyrell, out var score) ? score : 0;
    public double Greyjoy => Houses.TryGetValue(House.Greyjoy, out var score) ? score : 0;
    public double Martell => Houses.TryGetValue(House.Martell, out var score) ? score : 0;
    public double Arryn => Houses.TryGetValue(House.Arryn, out var score) ? score : 0;
}

public record HouseScoreDetailsViewModel(
    double Points,
    double Wins,
    double Cla,
    double Supplies,
    double PowerTokens,
    double Moves,
    Stats? Stats)
{
    public Stats Stats { get; } = Stats ?? new Stats();
}

public class SummaryInteractionsViewModel : Dictionary<ScoreType, TotalInteractions> { }