using System.Text.Json.Serialization;

namespace TMLeague.Models;

public record Season(
    [property: JsonPropertyName("startDate")] DateTimeOffset StartDate,
    [property: JsonPropertyName("endDate")] DateTimeOffset EndDate);