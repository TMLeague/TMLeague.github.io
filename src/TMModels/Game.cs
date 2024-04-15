using System.Text.Json.Serialization;

namespace TMModels;

public record Game(
    int Id,
    string Name,
    bool IsFinished,
    bool IsStalling,
    int Turn,
    Map Map,
    WesterosStats? Westeros,
    RavenAction?[]? Ravens,
    HouseScore[] Houses,
    DateTimeOffset GeneratedTime,
    bool IsCreatedManually = false)
{
    public double Progress => IsFinished ?
        100 : IsStalling ?
            97 : 100 * (double)Turn / 11;
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

public record WesterosStats(
    WesterosPhase1[][] Phase1,
    WesterosPhase2[][] Phase2,
    WesterosPhase3[][] Phase3,
    Wildling[][] Wildlings,
    int Turn)
{
    public void AddPhase1(int turn, WesterosPhase1 @event) =>
        Phase1[turn - 1] = Phase1[turn - 1].Append(@event).Distinct().ToArray();

    public void AddPhase2(int turn, WesterosPhase2 @event) =>
        Phase2[turn - 1] = Phase2[turn - 1].Append(@event).Distinct().ToArray();

    public void AddPhase3(int turn, WesterosPhase3 @event) =>
        Phase3[turn - 1] = Phase3[turn - 1].Append(@event).Distinct().ToArray();

    public void AddWildling(int turn, Wildling @event) =>
        Wildlings[turn - 1] = Wildlings[turn - 1].Append(@event).Distinct().ToArray();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WesterosPhase1
{
    Supply, Mustering, ThroneOfBlades, LastDaysOfSummer, WinterIsComing
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WesterosPhase2
{
    ClashOfKings, GameOfThrones, DarkWingsDarkWords, LastDaysOfSummer, WinterIsComing
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WesterosPhase3
{
    WildlingAttack, SeaOfStorms, RainsOfAutumn, FeastForCrows, WebOfLies, StormOfSwords, PutToTheSword
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Wildling
{
    AKingBeyondTheWall, CrowKillers, MammothRiders, MassingOnTheMilkwater, PreemptiveRaid,
    RattleshirtsRaiders, SilenceAtTheWall, SkinchangerScout, TheHordeDescends
}

public record Card(string Name, string Description);

public static class Cards
{
    public static readonly IReadOnlyDictionary<WesterosPhase1, Card> WesterosPhase1 =
        new Dictionary<WesterosPhase1, Card>
        {
            [TMModels.WesterosPhase1.Supply] = new("Supply", "Supplies update"),
            [TMModels.WesterosPhase1.Mustering] = new("Mustering", "Mustering units"),
            [TMModels.WesterosPhase1.ThroneOfBlades] = new("A Throne of Blades (W)", "The holder of the Iron Throne chose"),
            [TMModels.WesterosPhase1.LastDaysOfSummer] = new("Last Days of Summer (W)", "Nothing"),
            [TMModels.WesterosPhase1.WinterIsComing] = new("Winter is Coming", "Reshuffle the deck")
        };

    public static readonly IReadOnlyDictionary<WesterosPhase2, Card> WesterosPhase2 =
        new Dictionary<WesterosPhase2, Card>
        {
            [TMModels.WesterosPhase2.ClashOfKings] = new("Clash of Kings", "Influence Track Bidding"),
            [TMModels.WesterosPhase2.GameOfThrones] = new("Game of Thrones", "Gaining Power Tokens"),
            [TMModels.WesterosPhase2.DarkWingsDarkWords] = new("Dark Wings, Dark Words (W)", "The holder of the Messenger Raven chose"),
            [TMModels.WesterosPhase2.LastDaysOfSummer] = new("Last Days of Summer (W)", "Nothing"),
            [TMModels.WesterosPhase2.WinterIsComing] = new("Winter is Coming", "Reshuffle the deck")
        };

    public static readonly IReadOnlyDictionary<WesterosPhase3, Card> WesterosPhase3 =
        new Dictionary<WesterosPhase3, Card>
        {
            [TMModels.WesterosPhase3.WildlingAttack] = new("Wildlings Attack", "Wildlings Attack Bidding"),
            [TMModels.WesterosPhase3.SeaOfStorms] = new("Sea of Storms (W)", "No Raids Orders"),
            [TMModels.WesterosPhase3.RainsOfAutumn] = new("Rains of Autumn (W)", "No March+1 Order"),
            [TMModels.WesterosPhase3.FeastForCrows] = new("Feast for Crows (W)", "No Consolidate Power Orders"),
            [TMModels.WesterosPhase3.WebOfLies] = new("Web of Lies (W)", "No Support Orders"),
            [TMModels.WesterosPhase3.StormOfSwords] = new("Storm of Swords (W)", "No Defense Orders"),
            [TMModels.WesterosPhase3.PutToTheSword] = new("Put To the Sword", "The holder of the Valyrian Steel Blade chose")
        };

    public static readonly IReadOnlyDictionary<Wildling, Card> Wildlings =
        new Dictionary<Wildling, Card>
        {
            [Wildling.AKingBeyondTheWall] = new("A King Beyond the Wall",
                "<h5>Wildling Victory</h5><h6>Lowest Bidder</h6><div>Moves his tokens to the lowest position of every influence track</div><h6>Everyone else</h6><div>In turn order, each player chooses either the Fiefdoms or King's Court Influence track and moves his token to the lowest position of that track.</div><h5>Night's Watch Victory</h5><div>Moves his token to the top of one Influence track of his choice, then takes the appropriate Dominance token.</div>"),
            [Wildling.CrowKillers] = new("Crow Killers",
                "<h5>Wildling Victory</h5><h6>Lowest Bidder</h6><div>Replaces all of his Knights with available Footmen. Any Knight unable to be replaced is destroyed</div><h6>Everyone else</h6><div>Replaces 2 of their Knights with available Footmen. Any Knight unable to be replaced is destroyed.</div><h5>Night's Watch Victory</h5><div>May immediately replace up to 2 of his Footmen anywhere, with available Knights</div>"),
            [Wildling.MammothRiders] = new("Mammoth Riders",
                "<h5>Wildling Victory</h5><h6>Lowest Bidder</h6><div>Destroys 3 of his units anywhere</div><h6>Everyone else</h6><div>Destroys 2 of their units anywhere</div><h5>Night's Watch Victory</h5><div>May retrieve 1 House card of his choice from his House card discard pile</div>"),
            [Wildling.MassingOnTheMilkwater] = new("Massing on the Milkwater",
                "<h5>Wildling Victory</h5><h6>Lowest Bidder</h6><div>If he has more than one Hosue card in his hand, he discards all cards with the highest combat strength</div><h6>Everyone else</h6><div>If they have more than one House card in their hand, they must choose and discard one of those cards</div><h5>Night's Watch Victory</h5><div>Returns his entire House card discard pile into his hand</div>"),
            [Wildling.PreemptiveRaid] = new("Preemptive Raid",
                "<h5>Wildling Victory</h5><h6>Lowest Bidder</h6><div>Chooses one of the following: <br/>A) Destroys 2 of his units anywhere<br/>B) Is reduced 2 positions on his highest influence track</div><h6>Everyone else</h6><div>Nothing happens</div><h5>Night's Watch Victory</h5><div>The wildlings immediately attack again with a strength of 6. You do not participate in the bidding against this attack (nor do you receive any rewards or penalties)</div>"),
            [Wildling.RattleshirtsRaiders] = new("Rattleshirt's Raiders",
                "<h5>Wildling Victory</h5><h6>Lowest Bidder</h6><div>Is reduced 2 positions on the Supply track (to no lower than 0); then reconcile armies to their new Supply limits</div><h6>Everyone else</h6><div>Is reduced 1 position on the Supply track (to no lower than 0); then reconcile armies to their new Supply limits</div><h5>Night's Watch Victory</h5><div>Is increased 1 position on the Supply track (to no higher than 6)</div>"),
            [Wildling.SilenceAtTheWall] = new("Silence at the Wall",
                "<h5>Wildling Victory</h5><h6>Lowest Bidder</h6><div>Nothing happens</div><h6>Everyone else</h6><div>Nothing happens</div><h5>Night's Watch Victory</h5><div>Nothing happens</div>"),
            [Wildling.SkinchangerScout] = new("Skinchanger Scout",
                "<h5>Wildling Victory</h5><h6>Lowest Bidder</h6><div>Discards all available Power tokens</div><h6>Everyone else</h6><div>Discards 2 available Power tokens, or as many as they are able.</div><h5>Night's Watch Victory</h5><div>All Power tokens he bid on this attack are immediately returned to his available Power.</div>"),
            [Wildling.TheHordeDescends] = new("The Horde Descends",
                "<h5>Wildling Victory</h5><h6>Lowest Bidder</h6><div>Destroys 2 of his units at one of his Castles or Strongholds. If unable he destroys 2 of his units anywhere</div><h6>Everyone else</h6><div>Destroys 1 of their units anywyere</div><h5>Night's Watch Victory</h5><div>May muster forces (following normal mustering rules) in any one Castle or Stronghold area he controls</div>")
        };
}

public record RavenAction(RavenActionType Type, House House);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RavenActionType
{
    Nothing, Replaced, Look, Knows, Discarded
}

public record HouseScore(
    House House,
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
    int Turn,
    double? KnowsNextWildlings,
    Stats? Stats,
    PlayersInteractions? PlayersInteractions) : IComparable<HouseScore>
{
    public Stats Stats { get; } = Stats ?? new Stats();
    public string PlayerHouseName => string.IsNullOrEmpty(Player) ? House.ToString() : $"{Player} ({House})";

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

    public static Stats operator /(Stats stats, double divisor) => new(
         stats.Battles / divisor,
         stats.Kills / divisor,
         stats.Casualties / divisor,
         stats.PowerTokens / divisor,
         stats.Bids / divisor);
}

public record BattleStats
{
    public double SuccessfulAttacks { get; set; }
    public double SuccessfulDefenses { get; set; }
    [JsonIgnore]
    public double Won => SuccessfulAttacks + SuccessfulDefenses;
    public double LostAttacks { get; set; }
    public double LostDefenses { get; set; }
    [JsonIgnore]
    public double Lost => LostAttacks + LostDefenses;
    public Dictionary<House, double> Houses { get; set; } = new();
    [JsonIgnore]
    public double Total => Won + Lost;

    public BattleStats() { }
    public BattleStats(double successfulAttacks, double successfulDefenses, double lostAttacks, double lostDefenses, Dictionary<House, double> houses)
    {
        SuccessfulAttacks = successfulAttacks;
        SuccessfulDefenses = successfulDefenses;
        LostAttacks = lostAttacks;
        LostDefenses = lostDefenses;
        Houses = houses;
    }

    public static BattleStats Max(BattleStats stats1, BattleStats stats2) => new(
        Math.Max(stats1.SuccessfulAttacks, stats2.SuccessfulAttacks),
        Math.Max(stats1.SuccessfulDefenses, stats2.SuccessfulDefenses),
        Math.Min(stats1.LostAttacks, stats2.LostAttacks),
        Math.Min(stats1.LostDefenses, stats2.LostDefenses),
        stats1.Houses.Keys.Concat(stats2.Houses.Keys).Distinct()
            .ToDictionary(house => house, house => Math.Max(
                stats1.Houses.TryGetValue(house, out var battles1) ? battles1 : 0,
                stats2.Houses.TryGetValue(house, out var battles2) ? battles2 : 0)));

    public static BattleStats operator +(BattleStats stats1, BattleStats stats2) => new(
        stats1.SuccessfulAttacks + stats2.SuccessfulAttacks,
        stats1.SuccessfulDefenses + stats2.SuccessfulDefenses,
        stats1.LostAttacks + stats2.LostAttacks,
        stats1.LostDefenses + stats2.LostDefenses,
        stats1.Houses.Keys.Concat(stats2.Houses.Keys).Distinct()
            .ToDictionary(house => house, house =>
                stats1.Houses.TryGetValue(house, out var battles1) ? battles1 : 0
                    + (stats2.Houses.TryGetValue(house, out var battles2) ? battles2 : 0)));

    public static BattleStats operator /(BattleStats stats, double divisor) => new(
        stats.SuccessfulAttacks / divisor,
        stats.SuccessfulDefenses / divisor,
        stats.LostAttacks / divisor,
        stats.LostDefenses / divisor,
        stats.Houses.ToDictionary(pair => pair.Key, pair => pair.Value / divisor));
}

public record UnitStats
{
    public UnitStats() { }
    public UnitStats(double footmen, double knights, double siegeEngines, double ships)
    {
        Footmen = footmen;
        Knights = knights;
        SiegeEngines = siegeEngines;
        Ships = ships;
    }

    public double Footmen { get; set; }
    public double Knights { get; set; }
    public double SiegeEngines { get; set; }
    public double Ships { get; set; }
    [JsonIgnore]
    public double Total => Footmen + Knights + SiegeEngines + Ships;
    [JsonIgnore]
    public double MobilizationPoints => Footmen + 2 * Knights + 2 * SiegeEngines + Ships;

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

    public static UnitStats operator /(UnitStats stats, double divisor) => new(
        stats.Footmen / divisor,
        stats.Knights / divisor,
        stats.SiegeEngines / divisor,
        stats.Ships / divisor);
}

public record PowerTokenStats
{
    public double ConsolidatePower { get; set; }
    public double Raids { get; set; }
    public double GameOfThrones { get; set; }
    public double Wildlings { get; set; }
    public double Tywin { get; set; }
    [JsonIgnore]
    public double Total => ConsolidatePower + Raids + GameOfThrones + Wildlings + Tywin;

    public PowerTokenStats() { }
    public PowerTokenStats(double consolidatePower, double raids, double gameOfThrones, double wildlings, double tywin)
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

    public static PowerTokenStats operator /(PowerTokenStats stats, double divisor) => new(
        stats.ConsolidatePower / divisor,
        stats.Raids / divisor,
        stats.GameOfThrones / divisor,
        stats.Wildlings / divisor,
        stats.Tywin / divisor);
}

public record BidStats
{
    public double IronThrone { get; set; }
    public double Fiefdoms { get; set; }
    public double KingsCourt { get; set; }
    public double Wildlings { get; set; }
    public double Aeron { get; set; }
    [JsonIgnore]
    public double Total => IronThrone + Fiefdoms + KingsCourt + Wildlings + Aeron;

    public BidStats() { }
    public BidStats(double ironThrone, double fiefdoms, double kingsCourt, double wildlings, double aeron)
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

    public static BidStats operator /(BidStats stats, double divisor) => new(
        stats.IronThrone / divisor,
        stats.Fiefdoms / divisor,
        stats.KingsCourt / divisor,
        stats.Wildlings / divisor,
        stats.Aeron / divisor);
}

public class WildligKnowledge : Dictionary<House, HouseWildligKnowledge>
{
    public WildligKnowledge(IDictionary<House, HouseWildligKnowledge> dictionary) : base(dictionary) { }
}

public record HouseWildligKnowledge(bool Knows, int KnownWildlings)
{
    public HouseWildligKnowledge Discarded() => this with { Knows = false };
    public HouseWildligKnowledge Know() => new(true, Knows ? KnownWildlings : KnownWildlings + 1);
    public HouseWildligKnowledge Attack() => new(false, Knows ? KnownWildlings : KnownWildlings + 1);
}

public class PlayersInteractions : Dictionary<string, PlayerInteractions> { }
public class HousesInteractions : Dictionary<House, Interactions> { }

public record PlayerInteractions(
    string Player,
    House House) : Interactions
{
    public double Games { get; set; }
    public double Neighbors { get; set; }
    public Dictionary<string, double> Houses { get; set; } = new();
}

public record Interactions
{
    public double SuccessfulAttacks { get; set; }
    public double SuccessfulDefenses { get; set; }
    public double LostAttacks { get; set; }
    public double LostDefenses { get; set; }
    public double Supports { get; set; }
    public double SupportsOpponent { get; set; }
    public double WasSupported { get; set; }
    public double WasSupportedOpponent { get; set; }
    public double Raids { get; set; }
    public double WasRaided { get; set; }
    public double FavorsInTie { get; set; }
    public double WasFavoredInTie { get; set; }

    public Interactions()
    { }

    public Interactions(double successfulAttacks, double successfulDefenses, double lostAttacks, double lostDefenses,
        double supports, double supportsOpponent, double wasSupported, double wasSupportedOpponent,
        double raids, double wasRaided, double favorsInTie, double wasFavoredInTie)
    {
        SuccessfulAttacks = successfulAttacks;
        SuccessfulDefenses = successfulDefenses;
        LostAttacks = lostAttacks;
        LostDefenses = lostDefenses;
        Supports = supports;
        SupportsOpponent = supportsOpponent;
        WasSupported = wasSupported;
        WasSupportedOpponent = wasSupportedOpponent;
        Raids = raids;
        WasRaided = wasRaided;
        FavorsInTie = favorsInTie;
        WasFavoredInTie = wasFavoredInTie;
    }
}