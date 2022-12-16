using System.Text.Json.Serialization;

namespace TMModels;

public record Results(
    [property: JsonPropertyName("players")] PlayerResult[] Players,
    [property: JsonPropertyName("modifiedDate")] DateTimeOffset ModifiedDate
);

public record PlayerResult(
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("totalPoints")] short TotalPoints,
    [property: JsonPropertyName("wins")] ushort Wins,
    [property: JsonPropertyName("cla")] ushort Cla,
    [property: JsonPropertyName("supplies")] ushort Supplies,
    [property: JsonPropertyName("powerTokens")] ushort PowerTokens,
    [property: JsonPropertyName("minutesPerMove")] double MinutesPerMove,
    [property: JsonPropertyName("moves")] ushort Moves,
    [property: JsonPropertyName("houses")] HouseResult[] Houses,
    [property: JsonPropertyName("penaltiesPoints")] ushort PenaltiesPoints,
    [property: JsonPropertyName("penaltiesDetails")] PlayerPenalty[] PenaltiesDetails
);

public record HouseResult(
    [property: JsonPropertyName("game")] uint Game,
    [property: JsonPropertyName("house")] House House,
    [property: JsonPropertyName("isWinner")] bool IsWinner,
    [property: JsonPropertyName("points")] ushort Points,
    [property: JsonPropertyName("battlePenalty")] ushort BattlePenalty,
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
    [property: JsonPropertyName("points")] ushort Points,
    [property: JsonPropertyName("details")] string Details
);