using System.Text.Json.Serialization;

namespace TMModels;

public record Division(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("judge")] string Judge,
    [property: JsonPropertyName("players")] string[] Players,
    [property: JsonPropertyName("games")] int[] Games,
    [property: JsonPropertyName("penalties")] Penalty[]? Penalties,
    [property: JsonPropertyName("replacements")] Replacement[]? Replacements,
    [property: JsonPropertyName("isFinished")] bool IsFinished,
    [property: JsonPropertyName("winnerTitle")] string? WinnerTitle);

public record Penalty(
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("game")] int? Game,
    [property: JsonPropertyName("points")] double Points,
    [property: JsonPropertyName("details")] string Details);

public record Replacement(
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("game")] int Game);