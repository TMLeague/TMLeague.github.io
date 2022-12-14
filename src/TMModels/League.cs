using System.Text.Json.Serialization;

namespace TMModels;

public record League(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("backgroundImage")] string? BackgroundImage,
    [property: JsonPropertyName("rules")] string? Rules,
    [property: JsonPropertyName("discord")] string? Discord,
    [property: JsonPropertyName("seasons")] string[] Seasons
    );