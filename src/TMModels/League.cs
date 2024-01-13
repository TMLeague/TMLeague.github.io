using System.Text.Json.Serialization;
using TMModels.Extensions;

namespace TMModels;

public record League(
    string Name,
    string? Description,
    string? BackgroundImage,
    string? Rules,
    string? Discord,
    string? JudgeTitle,
    string[] Seasons,
    string[] TrainingSeasons,
    LeagueDivision[] MainDivisions,
    Scoring? Scoring,
    InitialMessage? InitialMessage
)
{
    public IEnumerable<string> AllSeasons => TrainingSeasons.Concat(Seasons);

    public Navigation GetSeasonNavigation(string seasonId)
    {
        var idx = Seasons.GetIdx(seasonId);
        var first = idx > 0 ? Seasons.First() : null;
        var previous = idx > 1 ? Seasons[idx - 1] : null;
        var next = idx < Seasons.Length - 2 ? Seasons[idx + 1] : null;
        var last = idx < Seasons.Length - 1 ? Seasons.Last() : null;
        return new Navigation(first, previous, next, last);
    }
}

public record LeagueDivision(
    string Id,
    string Name,
    int? Promotions,
    int? Relegations);

public record Scoring(
    double PointsPerStronghold,
    double PointsPerCastle,
    double PointsPerWin,
    double PointsPerClearWin,
    double PointsPer2ndPlace,
    double PointsPer3rdPlace,
    int RequiredBattlesBefore10thTurn,
    Tiebreaker[] Tiebreakers);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Tiebreaker
{
    Wins, Penalties, Cla, Supplies, PowerTokens, MinutesPerMove
}

public static class Tiebreakers
{
    public static readonly Tiebreaker[] Default = { Tiebreaker.Wins, Tiebreaker.Penalties, Tiebreaker.Cla, Tiebreaker.Supplies };

    public static string Name(this Tiebreaker tiebreaker) => tiebreaker switch
    {
        Tiebreaker.Wins => "Wins",
        Tiebreaker.Penalties => "Penalties",
        Tiebreaker.Cla => "CLA",
        Tiebreaker.Supplies => "Supplies",
        Tiebreaker.PowerTokens => "Power Tokens",
        Tiebreaker.MinutesPerMove => "MPM",
        _ => ""
    };
}

public record InitialMessage(
    string Subject,
    string[] Body,
    int? PasswordLength);