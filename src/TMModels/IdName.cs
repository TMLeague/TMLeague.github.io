using System.Text.Json.Serialization;

namespace TMModels;

public record IdName(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name);