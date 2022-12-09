using System.Text.Json.Serialization;
using TMLeague.Models.Serialization;

namespace TMLeague.Models.Thronemaster;

public record State(
    [property: JsonPropertyName("time")]
    [property: JsonConverter(typeof(DateTimeOffsetJsonConverter))]
    DateTimeOffset Time,

    [property: JsonPropertyName("error")]
    long Error,

    [property: JsonPropertyName("data")]
    string[] Data,

    [property: JsonPropertyName("setup")]
    string[] Setup,

    [property: JsonPropertyName("stats")]
    string[] Stats);