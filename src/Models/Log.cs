using System.Text.Json.Serialization;

namespace TMLeague.Models;

public record Log(
    int Id,
    string Name,
    DateTimeOffset Created,
    bool IsProfessional,
    Settings Settings,
    List<LogItem> Logs);

public class Settings : Dictionary<string, bool> { }

public record LogItem(
    int? Id,
    int Turn,
    Phase Phase,
    string Player,
    string Message,
    DateTimeOffset? Date)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TimeSpan? Duration { get; set; }
}

public enum Phase
{
    Westeros, Planning, Raven, Raid, March, Battle, Power, GameEnd
}