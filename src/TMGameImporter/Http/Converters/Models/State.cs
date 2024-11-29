using TMModels;

namespace TMGameImporter.Http.Converters.Models;

internal record State(
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
    string? Chat,
    DateTimeOffset? LastActionTime);

internal record HouseSpeed(House House, double MinutesPerMove, int MovesCount);

internal record Area(bool IsEnabled, AreaType Type, int Id, string Name);

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
            .Where(row => row.Length >= 3)
            .ToDictionary(row => row[1], row => string.Join(",", row[2..])))
    { }
}