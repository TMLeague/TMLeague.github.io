using System.Text.Json.Serialization;

namespace TMModels;

public record Draft(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("table")] string[][] Table);