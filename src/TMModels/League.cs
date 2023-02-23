using System.Text.Json.Serialization;

namespace TMModels;

public record League(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("backgroundImage")] string? BackgroundImage,
    [property: JsonPropertyName("rules")] string? Rules,
    [property: JsonPropertyName("discord")] string? Discord,
    [property: JsonPropertyName("judgeTitle")] string? JudgeTitle,
    [property: JsonPropertyName("seasons")] string[] Seasons,
    [property: JsonPropertyName("trainingSeasons")] string[] TrainingSeasons,
    [property: JsonPropertyName("mainDivisions")] IdName[] MainDivisions,
    [property: JsonPropertyName("scoring")] Scoring? Scoring,
    [property: JsonPropertyName("initialMessage")] InitialMessage? InitialMessage
)
{
    public IEnumerable<string> AllSeasons => TrainingSeasons.Concat(Seasons);
}

public record Scoring(
    [property: JsonPropertyName("pointsPerStronghold")] ushort PointsPerStronghold,
    [property: JsonPropertyName("pointsPerCastle")] ushort PointsPerCastle,
    [property: JsonPropertyName("pointsPerWin")] ushort PointsPerWin,
    [property: JsonPropertyName("requiredBattlesBefore10thTurn")] ushort RequiredBattlesBefore10thTurn,
    [property: JsonPropertyName("tiebreakers")] Tiebreaker[] Tiebreakers
);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Tiebreaker
{
    Wins, Penalties, Cla, Supplies, PowerTokens, MinutesPerMove
}

public static class Tiebreakers
{
    public static readonly Tiebreaker[] Default = { Tiebreaker.Wins, Tiebreaker.Penalties, Tiebreaker.Cla, Tiebreaker.Supplies };
}

public record InitialMessage(
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("body")] string[] Body);