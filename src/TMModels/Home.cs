using System.Text.Json.Serialization;

namespace TMModels;

public record Home(
    [property: JsonPropertyName("leagues")] string[] Leagues
);