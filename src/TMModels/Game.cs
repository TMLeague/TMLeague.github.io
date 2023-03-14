using System.Text.Json.Serialization;

namespace TMModels;

public record Game(
    int Id,
    string Name,
    bool IsFinished,
    bool IsStalling,
    int Turn,
    Map Map,
    HouseScore[] Houses,
    DateTimeOffset GeneratedTime,
    bool IsCreatedManually = false);

public record HouseScore(
    [property: JsonPropertyName("name")] House House,
    string Player,
    int Throne,
    int Fiefdoms,
    int KingsCourt,
    int Supplies,
    int PowerTokens,
    int Strongholds,
    int Castles,
    int Cla,
    double MinutesPerMove,
    int Moves,
    int[] BattlesInTurn,
    Stats? Stats) : IComparable<HouseScore>
{
    public Stats Stats { get; } = Stats ?? new Stats();

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
    Land[] Lands,
    Sea[] Seas,
    Port[] Ports);

public record Land(
    bool IsEnabled,
    int Id,
    string Name,
    House House,
    int Footmen,
    int Knights,
    int SiegeEngines,
    int Tokens,
    int MobilizationPoints,
    int Supplies,
    int Crowns);

public record Sea(
    bool IsEnabled,
    int Id,
    string Name,
    House House,
    int Ships);

public record Port(
    bool IsEnabled,
    int Id,
    string Name,
    int Ships);

public record Stats(
    BattleStats Battles,
    UnitStats Kills,
    UnitStats Casualties,
    PowerTokenStats PowerTokens,
    BidStats Bids)
{
    public Stats() :
        this(new BattleStats(), new UnitStats(), new UnitStats(), new PowerTokenStats(), new BidStats())
    { }

    public static Stats operator +(Stats stats1, Stats stats2) => new()
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
    [JsonIgnore]
    public int Total => Won + Lost;

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
    [JsonIgnore]
    public int Total => Footmen + Knights + SiegeEngines + Ships;
    [JsonIgnore]
    public int MobilizationPoints => Footmen + 2 * Knights + 2 * SiegeEngines + Ships;

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
    public int Wildlings { get; set; }
    public int Tywin { get; set; }
    [JsonIgnore]
    public int Total => ConsolidatePower + Raids + GameOfThrones + Wildlings + Tywin;

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
    [JsonIgnore]
    public int Total => IronThrone + Fiefdoms + KingsCourt + Wildlings + Aeron;

    public static BidStats operator +(BidStats stats1, BidStats stats2) => new()
    {
        IronThrone = stats1.IronThrone + stats2.IronThrone,
        Fiefdoms = stats1.Fiefdoms + stats2.Fiefdoms,
        KingsCourt = stats1.KingsCourt + stats2.KingsCourt,
        Wildlings = stats1.Wildlings + stats2.Wildlings
    };
}