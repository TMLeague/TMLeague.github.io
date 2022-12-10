using System.Text.Json.Serialization;

namespace TMLeague.Models
{
    public record Home(
        [property: JsonPropertyName("leagues")] string[]? Leagues
        );
}
