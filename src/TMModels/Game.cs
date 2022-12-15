using System.Text.Json.Serialization;

namespace TMModels;

public record Game(
    [property: JsonPropertyName("id")] uint Id,
    [property: JsonPropertyName("isFinished")] bool IsFinished,
    [property: JsonPropertyName("isStalling")] bool IsStalling,
    [property: JsonPropertyName("turn")] uint Turn,
    [property: JsonPropertyName("map")] Map Map,
    [property: JsonPropertyName("houses")] HouseData[] Houses);

public record HouseData(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("throne")] ushort Throne,
    [property: JsonPropertyName("fiefdoms")] ushort Fiefdoms,
    [property: JsonPropertyName("kingsCourt")] ushort KingsCourt,
    [property: JsonPropertyName("supplies")] ushort Supplies,
    [property: JsonPropertyName("powerTokens")] ushort PowerTokens,
    [property: JsonPropertyName("strongholds")] ushort Strongholds,
    [property: JsonPropertyName("castles")] ushort Castles,
    [property: JsonPropertyName("cla")] ushort Cla) : IComparable<HouseData>
{
    public int CompareTo(HouseData? otherHouse)
    {
        if (otherHouse == null)
            return 1;

        if (Castles + Strongholds != otherHouse.Castles + otherHouse.Strongholds)
            return Castles + Strongholds - otherHouse.Castles + otherHouse.Strongholds;

        if (Cla != otherHouse.Cla)
            return Cla - otherHouse.Cla;

        if (Supplies != otherHouse.Supplies)
            return Supplies - otherHouse.Supplies;

        return otherHouse.Throne - Throne;
    }
}

public record Map(
    [property: JsonPropertyName("lands")] Land[] Lands,
    [property: JsonPropertyName("seas")] Sea[] Seas,
    [property: JsonPropertyName("ports")] Port[] Ports);

public record Land(
    [property: JsonPropertyName("isEnabled")] bool IsEnabled,
    [property: JsonPropertyName("id")] ushort Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("house")] House House,
    [property: JsonPropertyName("footmen")] ushort Footmen,
    [property: JsonPropertyName("knights")] ushort Knights,
    [property: JsonPropertyName("siegeEngines")] ushort SiegeEngines,
    [property: JsonPropertyName("tokens")] ushort Tokens,
    [property: JsonPropertyName("mobilizationPoints")] ushort MobilizationPoints,
    [property: JsonPropertyName("supplies")] ushort Supplies,
    [property: JsonPropertyName("crowns")] ushort Crowns);

public record Sea(
    [property: JsonPropertyName("isEnabled")] bool IsEnabled,
    [property: JsonPropertyName("id")] ushort Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("house")] House House,
    [property: JsonPropertyName("ships")] ushort Ships);

public record Port(
    [property: JsonPropertyName("isEnabled")] bool IsEnabled,
    [property: JsonPropertyName("id")] ushort Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("ships")] ushort Ships);