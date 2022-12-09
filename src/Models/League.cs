using System.Text.Json.Serialization;

namespace TMLeague.Models;

public record League(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("backgroundImage")] string? BackgroundImage,
    [property: JsonPropertyName("rules")] string? Rules,
    [property: JsonPropertyName("discord")] string? Discord);