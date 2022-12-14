using System.Text.Json.Serialization;
using TMModels.Serialization;

namespace TMModels.ThroneMaster;

public record State(
    [property: JsonPropertyName("time")]
    [property: JsonConverter(typeof(DateTimeOffsetJsonConverter))]
    DateTimeOffset Time,
    [property: JsonPropertyName("error")] long Error,
    [property: JsonPropertyName("notes")] string Notes,
    [property: JsonPropertyName("chat")] object[] Chat,
    [property: JsonPropertyName("data")] string[] Data,
    [property: JsonPropertyName("logs")] object[] Logs,
    [property: JsonPropertyName("online")] object[] Online,
    [property: JsonPropertyName("setup")] string[] Setup,
    [property: JsonPropertyName("stats")] string[] Stats);