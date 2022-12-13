using System.Text.Json.Serialization;

namespace TMModels;

public record Division(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("judge")] string Judge,
    [property: JsonPropertyName("players")] string[] Players,
    [property: JsonPropertyName("games")] uint[] Games,
    [property: JsonPropertyName("penalties")] Penalty[]? Penalties,
    [property: JsonPropertyName("isFinished")] bool IsFinished,
    [property: JsonPropertyName("results")] PlayerResult[]? Results);

public record Penalty(
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("game")] uint? Game,
    [property: JsonPropertyName("points")] int Points,
    [property: JsonPropertyName("details")] string Details);

public record PlayerResult(
    string Player,
    HouseResult[] Houses);

public record HouseResult(
    [property: JsonPropertyName("game")] string Game,
    [property: JsonPropertyName("points")] ushort Points,
    [property: JsonPropertyName("cla")] ushort Cla,
    [property: JsonPropertyName("supply")] ushort Supply,
    [property: JsonPropertyName("strongholds")] ushort Strongholds,
    [property: JsonPropertyName("castles")] ushort Castles,
    [property: JsonPropertyName("isWinner")] bool IsWinner
);