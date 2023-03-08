using System.Text.Json.Serialization;

namespace TMModels.ThroneMaster;

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

    public House House => Enum.TryParse<House>(Message.Split()[Message.StartsWith("Wildlings Attack: ") ? 2 : 0], out var house) ? house : House.Unknown;
}

public enum Phase
{
    Westeros, Planning, Raven, Raid, March, Battle, Power, GameEnd
}