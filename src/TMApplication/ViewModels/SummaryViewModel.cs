using System.Data.Common;
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

public enum SummaryColumn
{
    Default, Player, Points, Wins, Penalties, Cla, Supply, PT, Baratheon, Lannister, Stark, Tyrell, Greyjoy, Martell, Position, MPM, AllSeasons
}

public static class SummaryColumns
{
    public static IEnumerable<PlayerScoreViewModel> Sort(this IEnumerable<PlayerScoreViewModel> players, SummaryColumn column, bool sortAscending, ScoreType scoreType) =>
        column switch
        {
            SummaryColumn.Player => players.OrderBy(player => player.Player).SortWithDirection(sortAscending),
            SummaryColumn.Points => players.OrderBy(player => player.TotalPoints(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.Wins => players.OrderBy(player => player.Wins(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.Penalties => players.OrderBy(player => player.PenaltiesPoints(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.Cla => players.OrderBy(player => player.Cla(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.Supply => players.OrderBy(player => player.Supplies(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.PT => players.OrderBy(player => player.PowerTokens(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.Baratheon => players.OrderBy(player => player.Baratheon(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.Lannister => players.OrderBy(player => player.Lannister(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.Stark => players.OrderBy(player => player.Stark(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.Tyrell => players.OrderBy(player => player.Tyrell(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.Greyjoy => players.OrderBy(player => player.Greyjoy(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.Martell => players.OrderBy(player => player.Martell(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.Position => players.OrderBy(player => player.Position(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.MPM => players.OrderBy(player => player.MinutesPerMove(scoreType)).SortWithDirection(sortAscending),
            SummaryColumn.AllSeasons => players.OrderBy(player => player.Seasons).SortWithDirection(sortAscending),
            _ => players
        };

    private static IEnumerable<PlayerScoreViewModel> SortWithDirection(this IOrderedEnumerable<PlayerScoreViewModel> orderBy, bool sortAscending) =>
        sortAscending ? orderBy : orderBy.Reverse();

    public static bool GetSortAscendingDefault(this SummaryColumn column) => column switch
    {
        SummaryColumn.Player => true,
        SummaryColumn.Points => false,
        SummaryColumn.Wins => false,
        SummaryColumn.Penalties => true,
        SummaryColumn.Cla => false,
        SummaryColumn.Supply => false,
        SummaryColumn.PT => false,
        SummaryColumn.Baratheon => false,
        SummaryColumn.Lannister => false,
        SummaryColumn.Stark => false,
        SummaryColumn.Tyrell => false,
        SummaryColumn.Greyjoy => false,
        SummaryColumn.Martell => false,
        SummaryColumn.Position => true,
        SummaryColumn.MPM => true,
        SummaryColumn.AllSeasons => false,
        _ => false
    };
}