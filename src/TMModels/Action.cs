using TMModels.ThroneMaster;

namespace TMModels;

public record Action(string Player, Phase Phase, DateTime Timestamp)
{
    public DateTime Date => Timestamp.Date;
    public TimeSpan Time => Timestamp.TimeOfDay;
}
