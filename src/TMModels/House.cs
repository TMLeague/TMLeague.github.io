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