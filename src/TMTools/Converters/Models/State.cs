using TMModels;

namespace TMTools.Converters.Models;

public record State(
    int? GameId,
    string Name,
    DateTimeOffset Time,
    bool IsFinished,
    int Turn,
    string HousesDataRaw,
    House[] HousesOrder,
    string[] Players,
    Map Map,
    HouseSpeed[] Stats,
    string? Chat);

public record HouseSpeed(House House, double MinutesPerMove, int MovesCount);

public record Area(bool IsEnabled, AreaType Type, int Id, string Name);

public enum AreaType
{
    Unknown, Land, Sea, Port
}

public class Setup : StateDictionary
{
    public Setup(IEnumerable<string> array) :
        base(array)
    { }
}

public class Data : StateDictionary
{
    public Data(IEnumerable<string> array) :
        base(array)
    { }
}

public abstract class StateDictionary : Dictionary<string, string>
{
    protected StateDictionary(IEnumerable<string> array) :
        base(array.Select(row => row.Split(','))
            .Where(row => row.Length >= 3)
            .ToDictionary(row => row[1], row => string.Join(",", row[2..])))
    { }
}