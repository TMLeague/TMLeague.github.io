using System.Text.Json.Serialization;
using TMModels.Serialization;

namespace TMModels.ThroneMaster;

public record StateRaw(
    [property: JsonConverter(typeof(DateTimeOffsetJsonConverter))]
    DateTimeOffset Time,
    long Error,
    string Notes,
    object[] Chat,
    string[] Data,
    object[] Logs,
    object[] Online,
    string[] Setup,
    string[] Stats);