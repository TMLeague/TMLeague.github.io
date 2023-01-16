using System.Text.Json.Serialization;

namespace TMModels;

public record Division(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("judge")] string Judge,
    [property: JsonPropertyName("players")] string[] Players,
    [property: JsonPropertyName("games")] uint[] Games,
    [property: JsonPropertyName("penalties")] Penalty[]? Penalties,
    [property: JsonPropertyName("replacements")] Replacement[]? Replacements,
    [property: JsonPropertyName("isFinished")] bool IsFinished,
    [property: JsonPropertyName("winnerTitle")] string? WinnerTitle);

public record Penalty(
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("game")] uint? Game,
    [property: JsonPropertyName("points")] ushort Points,
    [property: JsonPropertyName("details")] string Details);

public record Replacement(
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("game")] uint Game);