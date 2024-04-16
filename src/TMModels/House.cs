using System.Text.Json.Serialization;

namespace TMModels;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum House
{
    Unknown,
    Baratheon,
    Lannister,
    Stark,
    Tyrell,
    Greyjoy,
    Martell,
    Arryn
}

public static class HouseParser
{
    public static House Parse(string value) => Parse(value[0]);

    public static House Parse(char value) =>
        char.ToLower(value) switch
        {
            'b' => House.Baratheon,
            'l' => House.Lannister,
            's' => House.Stark,
            't' => House.Tyrell,
            'g' => House.Greyjoy,
            'm' => House.Martell,
            'a' => House.Arryn,
            _ => House.Unknown
        };
}

public static class Houses
{
    /// <summary>
    /// A dictionaries of houses that are neighboring to each other.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, Dictionary<House, House[]>> Neighbors = new Dictionary<int, Dictionary<House, House[]>>
    {
        [3] = new()
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark },
            [House.Lannister] = new[] { House.Baratheon, House.Stark },
            [House.Stark] = new[] { House.Baratheon, House.Lannister }
        },
        [4] = new()
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark, House.Tyrell },
            [House.Lannister] = new[] { House.Baratheon, House.Greyjoy, House.Tyrell },
            [House.Stark] = new[] { House.Baratheon, House.Greyjoy },
            [House.Tyrell] = new[] { House.Baratheon, House.Lannister },
            [House.Greyjoy] = new[] { House.Lannister, House.Stark }
        },
        [5] = new()
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark, House.Tyrell },
            [House.Lannister] = new[] { House.Baratheon, House.Greyjoy, House.Tyrell },
            [House.Stark] = new[] { House.Baratheon, House.Greyjoy },
            [House.Tyrell] = new[] { House.Baratheon, House.Lannister },
            [House.Greyjoy] = new[] { House.Lannister, House.Stark }
        },
        [6] = new()
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark, House.Martell }, // no Tyrell
            [House.Lannister] = new[] { House.Baratheon, House.Greyjoy, House.Tyrell },
            [House.Stark] = new[] { House.Baratheon, House.Greyjoy },
            [House.Tyrell] = new[] { House.Lannister, House.Martell }, // no Baratheon
            [House.Greyjoy] = new[] { House.Lannister, House.Stark },
            [House.Martell] = new[] { House.Baratheon, House.Tyrell }
        },
        [7] = new()
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark, House.Martell, House.Arryn },
            [House.Lannister] = new[] { House.Baratheon, House.Greyjoy, House.Tyrell },
            [House.Stark] = new[] { House.Baratheon, House.Greyjoy, House.Arryn },
            [House.Tyrell] = new[] { House.Lannister, House.Martell },
            [House.Greyjoy] = new[] { House.Lannister, House.Stark, House.Arryn },
            [House.Martell] = new[] { House.Baratheon, House.Tyrell },
            [House.Arryn] = new[] { House.Baratheon, House.Stark, House.Greyjoy }
        }
    };

    public static bool IsNeighbor(this House playerHouse, House otherHouse, int housesCount) =>
        playerHouse != House.Unknown && Neighbors[GetHousesCount(housesCount)][playerHouse].Contains(otherHouse);

    private static int GetHousesCount(int housesCount) => housesCount switch
    {
        < 3 => 3,
        > 7 => 7,
        _ => housesCount
    };
}