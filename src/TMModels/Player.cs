using System.Text.Json.Serialization;

namespace TMModels;

public record Player(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("rankPoints")] int RankPoints,
    [property: JsonPropertyName("country")] string Country, 
    [property: JsonPropertyName("location")] string Location, 
    [property: JsonPropertyName("speed")] string Speed);