using System.Text.Json.Serialization;

namespace TMLeague.Models;

public record Division(
    [property: JsonPropertyName("judge")] string Judge,
    [property: JsonPropertyName("players")] List<string> Players,
    [property: JsonPropertyName("games")] List<uint> Games,
    [property: JsonPropertyName("penalties")] List<Penalty> Penalties,
    [property: JsonPropertyName("isFinished")] bool IsFinished,
    [property: JsonPropertyName("results")] Dictionary<string, PlayerResult> Results);

public record Penalty(
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("game")] uint Game,
    [property: JsonPropertyName("points")] int Points,
    [property: JsonPropertyName("details")] string Details);

public class PlayerResult : Dictionary<string, HouseResult> { }

public record HouseResult(
    [property: JsonPropertyName("game")] string Game,
    [property: JsonPropertyName("points")] ushort Points,
    [property: JsonPropertyName("cla")] ushort Cla,
    [property: JsonPropertyName("supply")] ushort Supply,
    [property: JsonPropertyName("strongholds")] ushort Strongholds,
    [property: JsonPropertyName("castles")] ushort Castles,
    [property: JsonPropertyName("isWinner")] bool IsWinner
);