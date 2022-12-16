using System.Text.Json.Serialization;

namespace TMModels;

public record League(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("backgroundImage")] string? BackgroundImage,
    [property: JsonPropertyName("rules")] string? Rules,
    [property: JsonPropertyName("discord")] string? Discord,
    [property: JsonPropertyName("seasons")] string[] Seasons,
    [property: JsonPropertyName("scoring")] Scoring? Scoring
    );

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