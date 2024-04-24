using System.Text.Json.Serialization;
using TMModels.Extensions;

namespace TMModels;

public record TotalInteractions(
    Dictionary<string, PlayersInteractions> Players,
    Dictionary<House, HousesInteractions> Houses)
{
    public TotalInteractions() : this(
        new Dictionary<string, PlayersInteractions>(),
        new Dictionary<House, HousesInteractions>())
    { }

    public static TotalInteractions Max(TotalInteractions totalInteractions1, TotalInteractions totalInteractions2) => new(
        totalInteractions1.Players.OuterJoin(totalInteractions2.Players, PlayersInteractions.Max),
        totalInteractions1.Houses.OuterJoin(totalInteractions2.Houses, HousesInteractions.Max));

    public static TotalInteractions operator +(TotalInteractions totalInteractions1, TotalInteractions totalInteractions2) => new(
        totalInteractions1.Players.OuterJoin(totalInteractions2.Players, (i1, i2) => i1 + i2),
        totalInteractions1.Houses.OuterJoin(totalInteractions2.Houses, (i1, i2) => i1 + i2));

    public static TotalInteractions operator +(TotalInteractions totalInteractions, Game game) => new(
        totalInteractions.Players.OuterJoin(game.Houses.Where(score => !string.IsNullOrEmpty(score.Player)).ToDictionary(score => score.Player,
            score => score.PlayersInteractions), (i1, i2) => i1 + i2),
        totalInteractions.Houses.OuterJoin(game.Houses.ToDictionary(score => score.House,
            score => score.HousesInteractions), (i1, i2) => i1 + i2));

    public TotalInteractions Average() => new(
        Players.ToDictionary(pair => pair.Key, pair => pair.Value.Average()),
        Houses.ToDictionary(pair => pair.Key, pair => pair.Value.Average()));
}

public class PlayersInteractions : Dictionary<string, PlayerInteractions>
{
    public PlayersInteractions() { }
    public PlayersInteractions(IEnumerable<KeyValuePair<string, PlayerInteractions>> collection) : base(collection) { }

    public static PlayersInteractions Max(PlayersInteractions playersInteractions1, PlayersInteractions playersInteractions2) => new(
        playersInteractions1.OuterJoin(playersInteractions2, PlayerInteractions.Max));

    public static PlayersInteractions operator +(PlayersInteractions playersInteractions1, PlayersInteractions playersInteractions2) => new(
        playersInteractions1.OuterJoin(playersInteractions2, (i1, i2) => i1 + i2));

    public static PlayersInteractions operator /(PlayersInteractions interactions, double divisor) => new(
        interactions.ToDictionary(pair => pair.Key, pair => pair.Value / divisor));

    public PlayersInteractions Average() => new(this.ToDictionary(pair => pair.Key, pair => pair.Value.Average()));

    public void Import(House house, int players, HousesInteractions interactions, Dictionary<House, string> housePlayers, int? gameId)
    {
        foreach (var (enemyHouse, interaction) in interactions)
        {
            var enemy = housePlayers[enemyHouse];
            var housesKey = PlayerInteractions.GetHousesKey(house, enemyHouse);
            this[enemy] = new PlayerInteractions(enemy, house.IsNeighbor(enemyHouse, players) ? 1 : 0,
                gameId == null ? new Dictionary<string, List<int>>() : new Dictionary<string, List<int>> { [housesKey] = new() { gameId.Value } },
                interaction);
        }
    }
}

public class HousesInteractions : Dictionary<House, Interactions>
{
    public HousesInteractions() { }
    public HousesInteractions(IEnumerable<House> houses) : base(houses.ToDictionary(h => h, _ => new Interactions { Games = 1 })) { }
    public HousesInteractions(IDictionary<House, Interactions> dictionary) : base(dictionary) { }

    public static HousesInteractions Max(HousesInteractions housesInteractions1, HousesInteractions housesInteractions2) => new(
        housesInteractions1.OuterJoin(housesInteractions2, Interactions.Max));

    public static HousesInteractions operator +(HousesInteractions housesInteractions1, HousesInteractions housesInteractions2) => new(
        housesInteractions1.OuterJoin(housesInteractions2, (i1, i2) => i1 + i2));

    public static HousesInteractions operator /(HousesInteractions interactions, double divisor) => new(
        interactions.ToDictionary(pair => pair.Key, pair => pair.Value / divisor));

    public HousesInteractions Average() => new(this.ToDictionary(pair => pair.Key, pair => pair.Value.Average()));

    public void AddSuccessfulAttack(House enemy, UnitStats winnerCasualties, UnitStats looserCasualties, Dictionary<House, int> attackerSupporters, Dictionary<House, int> defenderSupporters)
    {
        this[enemy].SuccessfulAttacks++;
        this[enemy].Kills += looserCasualties;
        this[enemy].Casualties += winnerCasualties;
        foreach (var supporter in attackerSupporters.Keys)
            this[supporter].WasSupported++;
        foreach (var supporter in defenderSupporters.Keys)
            this[supporter].WasSupportedOpponent++;
    }

    public void AddSuccessfulDefense(House enemy, UnitStats winnerCasualties, UnitStats looserCasualties, Dictionary<House, int> attackerSupporters, Dictionary<House, int> defenderSupporters)
    {
        this[enemy].SuccessfulDefenses++;
        this[enemy].Kills += looserCasualties;
        this[enemy].Casualties += winnerCasualties;
        foreach (var supporter in defenderSupporters.Keys)
            this[supporter].WasSupported++;
        foreach (var supporter in attackerSupporters.Keys)
            this[supporter].WasSupportedOpponent++;
    }

    public void AddLostAttack(House enemy, UnitStats winnerCasualties, UnitStats looserCasualties, Dictionary<House, int> attackerSupporters, Dictionary<House, int> defenderSupporters)
    {
        this[enemy].LostAttacks++;
        this[enemy].Kills += winnerCasualties;
        this[enemy].Casualties += looserCasualties;
        foreach (var supporter in attackerSupporters.Keys)
            this[supporter].WasSupported++;
        foreach (var supporter in defenderSupporters.Keys)
            this[supporter].WasSupportedOpponent++;
    }

    public void AddLostDefenses(House enemy, UnitStats winnerCasualties, UnitStats looserCasualties, Dictionary<House, int> attackerSupporters, Dictionary<House, int> defenderSupporters)
    {
        this[enemy].LostDefenses++;
        this[enemy].Kills += winnerCasualties;
        this[enemy].Casualties += looserCasualties;
        foreach (var supporter in defenderSupporters.Keys)
            this[supporter].WasSupported++;
        foreach (var supporter in attackerSupporters.Keys)
            this[supporter].WasSupportedOpponent++;
    }

    public void AddSupport(House supportedHouse, House enemyHouse)
    {
        this[supportedHouse].Supports++;
        this[enemyHouse].SupportsOpponent++;
    }
}

public record PlayerInteractions()
{
    public string Player { get; init; } = string.Empty;
    public double Neighbors { get; set; }
    public Dictionary<string, List<int>> HousesGames { get; set; } = new();
    public Interactions Interactions { get; set; } = new();

    public PlayerInteractions(string player) : this()
    {
        Player = player;
    }

    public PlayerInteractions(string player, double neighbors, Dictionary<string, List<int>> housesGames, Interactions interactions) : this(player)
    {
        Neighbors = neighbors;
        HousesGames = housesGames;
        Interactions = interactions;
    }

    public static string GetHousesKey(House playerHouse, House enemyHouse) => $"{playerHouse.ToString()[0]}{enemyHouse.ToString()[0]}";

    public static PlayerInteractions Max(PlayerInteractions interactions1, PlayerInteractions interactions2) => new(
        interactions1.Player == interactions2.Player ? interactions1.Player : string.Empty,
        Math.Max(interactions1.Neighbors, interactions2.Neighbors),
        interactions1.HousesGames.OuterJoin(interactions2.HousesGames,
            (games1, games2) => games1.Concat(games2).ToList()),
        Interactions.Max(interactions1.Interactions, interactions2.Interactions));

    public static PlayerInteractions operator +(PlayerInteractions interactions1, PlayerInteractions interactions2) => new(
        interactions1.Player == interactions2.Player ? interactions1.Player : string.Empty,
        interactions1.Neighbors + interactions2.Neighbors,
        interactions1.HousesGames.OuterJoin(interactions2.HousesGames,
            (games1, games2) => games1.Concat(games2).ToList()),
        interactions1.Interactions + interactions2.Interactions);

    public static PlayerInteractions operator /(PlayerInteractions interactions, double divisor) => new(
        interactions.Player,
        interactions.Neighbors / divisor,
        interactions.HousesGames,
        interactions.Interactions / divisor);

    public PlayerInteractions Average() => this / (Interactions?.Games ?? 1);
}

public record Interactions
{
    public double Games { get; set; }
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
    public UnitStats Kills { get; set; } = new();
    public UnitStats Casualties { get; set; } = new();
    [JsonIgnore]
    public double Value => Supports + WasSupported + FavorsInTie + WasFavoredInTie - SuccessfulAttacks - LostDefenses - SupportsOpponent - WasSupportedOpponent - Raids - WasRaided;
    [JsonIgnore]
    public double Total => SuccessfulAttacks + SuccessfulDefenses + LostAttacks + LostDefenses + Supports + SupportsOpponent + WasSupported + WasSupportedOpponent + Raids + WasRaided + FavorsInTie + WasFavoredInTie;
    [JsonIgnore]
    public double Battles => SuccessfulAttacks + LostAttacks + SuccessfulDefenses + LostDefenses;
    [JsonIgnore]
    public double AllSupports => Supports + WasSupported;
    [JsonIgnore]
    public double AllSupportsOpponent => SupportsOpponent + WasSupportedOpponent;

    public Interactions()
    { }

    public Interactions(double games, double successfulAttacks, double successfulDefenses, double lostAttacks, double lostDefenses,
        double supports, double supportsOpponent, double wasSupported, double wasSupportedOpponent,
        double raids, double wasRaided, double favorsInTie, double wasFavoredInTie, UnitStats kills, UnitStats casualties)
    {
        Games = games;
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
        Kills = kills;
        Casualties = casualties;
    }

    public static Interactions Max(Interactions stats1, Interactions stats2) => new(
        Math.Max(stats1.Games, stats2.Games),
        Math.Max(stats1.SuccessfulAttacks, stats2.SuccessfulAttacks),
        Math.Max(stats1.SuccessfulDefenses, stats2.SuccessfulDefenses),
        Math.Max(stats1.LostAttacks, stats2.LostAttacks),
        Math.Max(stats1.LostDefenses, stats2.LostDefenses),
        Math.Max(stats1.Supports, stats2.Supports),
        Math.Max(stats1.SupportsOpponent, stats2.SupportsOpponent),
        Math.Max(stats1.WasSupported, stats2.WasSupported),
        Math.Max(stats1.WasSupportedOpponent, stats2.WasSupportedOpponent),
        Math.Max(stats1.Raids, stats2.Raids),
        Math.Max(stats1.WasRaided, stats2.WasRaided),
        Math.Max(stats1.FavorsInTie, stats2.FavorsInTie),
        Math.Max(stats1.WasFavoredInTie, stats2.WasFavoredInTie),
        UnitStats.Max(stats1.Kills, stats2.Kills),
        UnitStats.Max(stats1.Casualties, stats2.Casualties));

    public static Interactions operator +(Interactions stats1, Interactions stats2) => new(
        stats1.Games + stats2.Games,
        stats1.SuccessfulAttacks + stats2.SuccessfulAttacks,
        stats1.SuccessfulDefenses + stats2.SuccessfulDefenses,
        stats1.LostAttacks + stats2.LostAttacks,
        stats1.LostDefenses + stats2.LostDefenses,
        stats1.Supports + stats2.Supports,
        stats1.SupportsOpponent + stats2.SupportsOpponent,
        stats1.WasSupported + stats2.WasSupported,
        stats1.WasSupportedOpponent + stats2.WasSupportedOpponent,
        stats1.Raids + stats2.Raids,
        stats1.WasRaided + stats2.WasRaided,
        stats1.FavorsInTie + stats2.FavorsInTie,
        stats1.WasFavoredInTie + stats2.WasFavoredInTie,
        stats1.Kills + stats2.Kills,
        stats1.Casualties + stats2.Casualties);

    public static Interactions operator /(Interactions stats, double divisor) => new(
        stats.Games / divisor,
        stats.SuccessfulAttacks / divisor,
        stats.SuccessfulDefenses / divisor,
        stats.LostAttacks / divisor,
        stats.LostDefenses / divisor,
        stats.Supports / divisor,
        stats.SupportsOpponent / divisor,
        stats.WasSupported / divisor,
        stats.WasSupportedOpponent / divisor,
        stats.Raids / divisor,
        stats.WasRaided / divisor,
        stats.FavorsInTie / divisor,
        stats.WasFavoredInTie / divisor,
        stats.Kills / divisor,
        stats.Casualties / divisor);

    public virtual Interactions Average() => this / Games;
}