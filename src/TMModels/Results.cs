using System.Text.Json.Serialization;

namespace TMModels;

public record Results(
    [property: JsonPropertyName("results")] PlayerResult[] Players,
    [property: JsonPropertyName("modifiedDate")] DateTimeOffset ModifiedDate
);

public record PlayerResult(
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("points")] short TotalPoints,
    [property: JsonPropertyName("penaltiesPoints")] ushort PenaltiesPoints,
    [property: JsonPropertyName("wins")] ushort Wins,
    [property: JsonPropertyName("cla")] ushort Cla,
    [property: JsonPropertyName("supplies")] ushort Supplies,
    [property: JsonPropertyName("powerTokens")] ushort PowerTokens,
    [property: JsonPropertyName("minutesPerMove")] double MinutesPerMove,
    [property: JsonPropertyName("moves")] ushort Moves,
    [property: JsonPropertyName("houses")] HouseResult[] Houses,
    [property: JsonPropertyName("penaltiesDetails")] PlayerPenalty[] PenaltiesDetails
);

public record HouseResult(
    [property: JsonPropertyName("game")] uint Game,
    [property: JsonPropertyName("isWinner")] bool IsWinner,
    [property: JsonPropertyName("points")] ushort Points,
    [property: JsonPropertyName("strongholds")] ushort Strongholds,
    [property: JsonPropertyName("castles")] ushort Castles,
    [property: JsonPropertyName("cla")] ushort Cla,
    [property: JsonPropertyName("supplies")] ushort Supplies,
    [property: JsonPropertyName("powerTokens")] ushort PowerTokens,
    [property: JsonPropertyName("minutesPerMove")] double MinutesPerMove,
    [property: JsonPropertyName("moves")] ushort Moves
);

public record PlayerPenalty(
    [property: JsonPropertyName("game")] uint? Game,
    [property: JsonPropertyName("points")] int Points,
    [property: JsonPropertyName("details")] string Details
);