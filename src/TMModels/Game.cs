using System.Text.Json.Serialization;
using TMModels.ThroneMaster;

namespace TMModels;

public record Game(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("isFinished")] bool IsFinished,
    [property: JsonPropertyName("isStalling")] bool IsStalling,
    [property: JsonPropertyName("turn")] int Turn,
    [property: JsonPropertyName("map")] Map Map,
    [property: JsonPropertyName("houses")] HouseScore[] Houses,
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
    [property: JsonPropertyName("battlesInTurn")] int[] BattlesInTurn,
     HouseStats Stats) : IComparable<HouseScore>
{
    [JsonPropertyName("stats")]
    public HouseStats Stats { get; } = Stats ?? new HouseStats();

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

public record HouseStats(BattleStats Battles, UnitStats Kills, UnitStats Casualties, PowerTokenStats PowerTokens, BidStats Bids)
{
    public HouseStats() :
        this(new BattleStats(), new UnitStats(), new UnitStats(), new PowerTokenStats(), new BidStats())
    { }

    public static HouseStats operator +(HouseStats stats1, HouseStats stats2) => new()
    {
        Battles = stats1.Battles + stats2.Battles,
        Kills = stats1.Kills + stats2.Kills,
        Casualties = stats1.Casualties + stats2.Casualties,
        PowerTokens = stats1.PowerTokens + stats2.PowerTokens,
        Bids = stats1.Bids + stats2.Bids
    };
}

public record BattleStats
{
    public int Won { get; set; }
    public int Lost { get; set; }

    public static BattleStats operator +(BattleStats stats1, BattleStats stats2) => new()
    {
        Won = stats1.Won + stats2.Won,
        Lost = stats1.Lost + stats2.Lost
    };
}

public record UnitStats
{
    public UnitStats() { }
    public UnitStats(int footmen, int knights, int siegeEngines, int ships)
    {
        Footmen = footmen;
        Knights = knights;
        SiegeEngines = siegeEngines;
        Ships = ships;
    }

    public int Footmen { get; set; }
    public int Knights { get; set; }
    public int SiegeEngines { get; set; }
    public int Ships { get; set; }

    public static UnitStats operator +(UnitStats stats1, UnitStats stats2) => new()
    {
        Footmen = stats1.Footmen + stats2.Footmen,
        Knights = stats1.Knights + stats2.Knights,
        SiegeEngines = stats1.SiegeEngines + stats2.SiegeEngines,
        Ships = stats1.Ships + stats2.Ships
    };
}

public record PowerTokenStats
{
    public int ConsolidatePower { get; set; }
    public int Raids { get; set; }
    public int GameOfThrones { get; set; }
    public int Tywin { get; set; }

    public static PowerTokenStats operator +(PowerTokenStats stats1, PowerTokenStats stats2) => new()
    {
        ConsolidatePower = stats1.ConsolidatePower + stats2.ConsolidatePower,
        Raids = stats1.Raids + stats2.Raids,
        GameOfThrones = stats1.GameOfThrones + stats2.GameOfThrones
    };
}

public record BidStats
{
    public int IronThrone { get; set; }
    public int Fiefdoms { get; set; }
    public int KingsCourt { get; set; }
    public int Wildlings { get; set; }
    public int Aeron { get; set; }

    public static BidStats operator +(BidStats stats1, BidStats stats2) => new()
    {
        IronThrone = stats1.IronThrone + stats2.IronThrone,
        Fiefdoms = stats1.Fiefdoms + stats2.Fiefdoms,
        KingsCourt = stats1.KingsCourt + stats2.KingsCourt,
        Wildlings = stats1.Wildlings + stats2.Wildlings
    };
}