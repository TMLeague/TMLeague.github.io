using System.Text.Json.Serialization;

namespace TMLeague.Models;

public record Season(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("startDate")] DateTimeOffset StartDate,
    [property: JsonPropertyName("endDate")] DateTimeOffset EndDate,
    [property: JsonPropertyName("divisions")] string[]? Divisions
    );