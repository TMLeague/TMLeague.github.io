using TMModels;

namespace TMGameImporter.Http.Converters.Models;

internal record State(
    uint? GameId,
    string Name,
    DateTimeOffset Time,
    bool IsFinished,
    uint Turn,
    string HousesDataRaw,
    House[] HousesOrder,
    string[] Players,
    Map Map,
    HouseSpeed[] Stats,
    string? Chat);

internal record HouseSpeed(House House, double MinutesPerMove, ushort MovesCount);

internal record Area(bool IsEnabled, AreaType Type, ushort Id, string Name);

internal enum AreaType
{
    Unknown, Land, Sea, Port
}

internal class Setup : StateDictionary
{
    public Setup(IEnumerable<string> array) :
        base(array)
    { }
}

internal class Data : StateDictionary
{
    public Data(IEnumerable<string> array) :
        base(array)
    { }
}

internal abstract class StateDictionary : Dictionary<string, string>
{
    protected StateDictionary(IEnumerable<string> array) :
        base(array.Select(row => row.Split(','))
            .Where(row => row.Length == 3)
            .ToDictionary(row => row[1], row => row[2]))
    { }
}