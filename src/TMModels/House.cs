namespace TMModels;

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
    public static House Parse(string value) =>
        value.ToLower()[0] switch
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