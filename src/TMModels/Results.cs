using System.Text.Json.Serialization;

namespace TMModels;

public record Results(
    [property: JsonPropertyName("players")] PlayerResult[] Players,
    [property: JsonPropertyName("titles")] PlayerTitle[] Titles,
    [property: JsonPropertyName("generatedTime")] DateTimeOffset GeneratedTime
);

public record PlayerResult(
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("totalPoints")] double TotalPoints,
    [property: JsonPropertyName("wins")] int Wins,
    [property: JsonPropertyName("cla")] int Cla,
    [property: JsonPropertyName("supplies")] int Supplies,
    [property: JsonPropertyName("powerTokens")] int PowerTokens,
    [property: JsonPropertyName("minutesPerMove")] double MinutesPerMove,
    [property: JsonPropertyName("moves")] int Moves,
    [property: JsonPropertyName("houses")] HouseResult[] Houses,
    [property: JsonPropertyName("penaltiesPoints")] double PenaltiesPoints,
    [property: JsonPropertyName("penaltiesDetails")] PlayerPenalty[] PenaltiesDetails
);

public record HouseResult(
    [property: JsonPropertyName("game")] int Game,
    [property: JsonPropertyName("house")] House House,
    [property: JsonPropertyName("isWinner")] bool IsWinner,
    [property: JsonPropertyName("points")] double Points,
    [property: JsonPropertyName("battlePenalty")] int BattlePenalty,
    [property: JsonPropertyName("strongholds")] int Strongholds,
    [property: JsonPropertyName("castles")] int Castles,
    [property: JsonPropertyName("cla")] int Cla,
    [property: JsonPropertyName("supplies")] int Supplies,
    [property: JsonPropertyName("powerTokens")] int PowerTokens,
    [property: JsonPropertyName("minutesPerMove")] double MinutesPerMove,
    [property: JsonPropertyName("moves")] int Moves
);

public record PlayerPenalty(
    [property: JsonPropertyName("game")] int? Game,
    [property: JsonPropertyName("points")] double Points,
    [property: JsonPropertyName("details")] string Details
);

public record PlayerTitle(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("details")] string Details,
    [property: JsonPropertyName("values")] double Values);