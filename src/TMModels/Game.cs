using System.Text.Json.Serialization;

namespace TMModels;

public record Game(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("isFinished")] bool IsFinished,
    [property: JsonPropertyName("isStalling")] bool IsStalling,
    [property: JsonPropertyName("turn")] int Turn,
    [property: JsonPropertyName("map")] Map Map,
    [property: JsonPropertyName("houses")] HouseScore[] Houses,
    [property: JsonPropertyName("winner")] string? Winner,
    [property: JsonPropertyName("generatedTime")] DateTimeOffset GeneratedTime);

public record HouseScore(
    [property: JsonPropertyName("name")] House House,
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("throne")] int Throne,
    [property: JsonPropertyName("fiefdoms")] int Fiefdoms,
    [property: JsonPropertyName("kingsCourt")] int KingsCourt,
    [property: JsonPropertyName("supplies")] int Supplies,
    [property: JsonPropertyName("powerTokens")] int PowerTokens,
    [property: JsonPropertyName("strongholds")] int Strongholds,
    [property: JsonPropertyName("castles")] int Castles,
    [property: JsonPropertyName("cla")] int Cla,
    [property: JsonPropertyName("minutesPerMove")] double MinutesPerMove,
    [property: JsonPropertyName("moves")] int Moves,
    [property: JsonPropertyName("battlesInTurn")] int[] BattlesInTurn) : IComparable<HouseScore>
{
    public int CompareTo(HouseScore? otherHouse)
    {
        if (otherHouse == null)
            return 1;

        if (Castles + Strongholds != otherHouse.Castles + otherHouse.Strongholds)
            return Castles + Strongholds - (otherHouse.Castles + otherHouse.Strongholds);

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
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("house")] House House,
    [property: JsonPropertyName("footmen")] int Footmen,
    [property: JsonPropertyName("knights")] int Knights,
    [property: JsonPropertyName("siegeEngines")] int SiegeEngines,
    [property: JsonPropertyName("tokens")] int Tokens,
    [property: JsonPropertyName("mobilizationPoints")] int MobilizationPoints,
    [property: JsonPropertyName("supplies")] int Supplies,
    [property: JsonPropertyName("crowns")] int Crowns);

public record Sea(
    [property: JsonPropertyName("isEnabled")] bool IsEnabled,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("house")] House House,
    [property: JsonPropertyName("ships")] int Ships);

public record Port(
    [property: JsonPropertyName("isEnabled")] bool IsEnabled,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("ships")] int Ships);