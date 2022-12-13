using System.Text.Json.Serialization;

namespace TMModels.ThroneMaster;

public record Log(
    uint Id,
    string Name,
    DateTimeOffset Created,
    bool IsProfessional,
    Settings Settings,
    List<LogItem> Logs);

public class Settings : Dictionary<string, bool> { }

public record LogItem(
    uint? Id,
    uint Turn,
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