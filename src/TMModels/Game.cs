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

    public static Stats Max(Stats stats1, Stats stats2) => new(
        BattleStats.Max(stats1.Battles, stats2.Battles),
        UnitStats.Max(stats1.Kills, stats2.Kills),
        UnitStats.Min(stats1.Casualties, stats2.Casualties),
        PowerTokenStats.Max(stats1.PowerTokens, stats2.PowerTokens),
        BidStats.Max(stats1.Bids, stats2.Bids));

    public static Stats operator +(Stats stats1, Stats stats2) => new(
         stats1.Battles + stats2.Battles,
         stats1.Kills + stats2.Kills,
         stats1.Casualties + stats2.Casualties,
         stats1.PowerTokens + stats2.PowerTokens,
         stats1.Bids + stats2.Bids);
}

public record BattleStats
{
    public int Won { get; set; }
    public int Lost { get; set; }
    [JsonIgnore]
    public int Total => Won + Lost;

    public BattleStats() { }
    public BattleStats(int won, int lost)
    {
        Won = won;
        Lost = lost;
    }

    public static BattleStats Max(BattleStats stats1, BattleStats stats2) => new(
        Math.Max(stats1.Won, stats2.Won),
        Math.Min(stats1.Lost, stats2.Lost));

    public static BattleStats operator +(BattleStats stats1, BattleStats stats2) => new(
        stats1.Won + stats2.Won,
        stats1.Lost + stats2.Lost);
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

    public static UnitStats Max(UnitStats stats1, UnitStats stats2) => new(
        Math.Max(stats1.Footmen, stats2.Footmen),
        Math.Max(stats1.Knights, stats2.Knights),
        Math.Max(stats1.SiegeEngines, stats2.SiegeEngines),
        Math.Max(stats1.Ships, stats2.Ships));

    public static UnitStats Min(UnitStats stats1, UnitStats stats2) => new(
        Math.Min(stats1.Footmen, stats2.Footmen),
        Math.Min(stats1.Knights, stats2.Knights),
        Math.Min(stats1.SiegeEngines, stats2.SiegeEngines),
        Math.Min(stats1.Ships, stats2.Ships));

    public static UnitStats operator +(UnitStats stats1, UnitStats stats2) => new(
        stats1.Footmen + stats2.Footmen,
        stats1.Knights + stats2.Knights,
        stats1.SiegeEngines + stats2.SiegeEngines,
        stats1.Ships + stats2.Ships);
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

    public PowerTokenStats() { }
    public PowerTokenStats(int consolidatePower, int raids, int gameOfThrones, int wildlings, int tywin)
    {
        ConsolidatePower = consolidatePower;
        Raids = raids;
        GameOfThrones = gameOfThrones;
        Wildlings = wildlings;
        Tywin = tywin;
    }

    public static PowerTokenStats Max(PowerTokenStats stats1, PowerTokenStats stats2) => new(
        Math.Max(stats1.ConsolidatePower, stats2.ConsolidatePower),
        Math.Max(stats1.Raids, stats2.Raids),
        Math.Max(stats1.GameOfThrones, stats2.GameOfThrones),
        Math.Max(stats1.Wildlings, stats2.Wildlings),
        Math.Max(stats1.Tywin, stats2.Tywin));

    public static PowerTokenStats operator +(PowerTokenStats stats1, PowerTokenStats stats2) => new(
        stats1.ConsolidatePower + stats2.ConsolidatePower,
        stats1.Raids + stats2.Raids,
        stats1.GameOfThrones + stats2.GameOfThrones,
        stats1.Wildlings + stats2.Wildlings,
        stats1.Tywin + stats2.Tywin);
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

    public BidStats() { }
    public BidStats(int ironThrone, int fiefdoms, int kingsCourt, int wildlings, int aeron)
    {
        IronThrone = ironThrone;
        Fiefdoms = fiefdoms;
        KingsCourt = kingsCourt;
        Wildlings = wildlings;
        Aeron = aeron;
    }

    public static BidStats Max(BidStats stats1, BidStats stats2) => new(
        Math.Max(stats1.IronThrone, stats2.IronThrone),
        Math.Max(stats1.Fiefdoms, stats2.Fiefdoms),
        Math.Max(stats1.KingsCourt, stats2.KingsCourt),
        Math.Max(stats1.Wildlings, stats2.Wildlings),
        Math.Max(stats1.Aeron, stats2.Aeron));

    public static BidStats operator +(BidStats stats1, BidStats stats2) => new(
        stats1.IronThrone + stats2.IronThrone,
        stats1.Fiefdoms + stats2.Fiefdoms,
        stats1.KingsCourt + stats2.KingsCourt,
        stats1.Wildlings + stats2.Wildlings,
        stats1.Aeron + stats2.Aeron);
}