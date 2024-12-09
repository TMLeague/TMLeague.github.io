using TMModels;

namespace TMApplication.ViewModels;

public record GameViewModel(
    int Id,
    string Name,
    bool IsFinished,
    bool IsStalling,
    bool IsCreatedManually,
    int Turn,
    double Progress,
    HouseScore[] Houses,
    WesterosProbabilities? Westeros,
    DateTimeOffset GeneratedTime,
    DateTimeOffset? LastActionTime)
{
    public bool IsWarning =>
        !IsFinished && (
            IsStalling && TimeSinceLastAction.Days >= 10 ||
            !IsStalling && TimeSinceLastAction.Days >= 3);

    public TimeSpan TimeSinceLastAction => LastActionTime.HasValue ? DateTimeOffset.UtcNow - LastActionTime.Value : TimeSpan.Zero;
}

public record WesterosDeck(
    Dictionary<WesterosPhase1, int> Phase1,
    Dictionary<WesterosPhase2, int> Phase2,
    Dictionary<WesterosPhase3, int> Phase3,
    Dictionary<Wildling, bool> Wildlings,
    int Turn)
{
    public static readonly IReadOnlyDictionary<WesterosPhase1, int> Phase1Full = new Dictionary<WesterosPhase1, int>
    {
        [WesterosPhase1.Supply] = 3,
        [WesterosPhase1.Mustering] = 3,
        [WesterosPhase1.ThroneOfBlades] = 2,
        [WesterosPhase1.LastDaysOfSummer] = 1,
        [WesterosPhase1.WinterIsComing] = 1
    };

    public static readonly IReadOnlyDictionary<WesterosPhase2, int> Phase2Full = new Dictionary<WesterosPhase2, int>
    {
        [WesterosPhase2.ClashOfKings] = 3,
        [WesterosPhase2.GameOfThrones] = 3,
        [WesterosPhase2.DarkWingsDarkWords] = 2,
        [WesterosPhase2.LastDaysOfSummer] = 1,
        [WesterosPhase2.WinterIsComing] = 1
    };

    public static readonly IReadOnlyDictionary<WesterosPhase3, int> Phase3Full = new Dictionary<WesterosPhase3, int>
    {
        [WesterosPhase3.WildlingAttack] = 3,
        [WesterosPhase3.SeaOfStorms] = 1,
        [WesterosPhase3.RainsOfAutumn] = 1,
        [WesterosPhase3.FeastForCrows] = 1,
        [WesterosPhase3.WebOfLies] = 1,
        [WesterosPhase3.StormOfSwords] = 1,
        [WesterosPhase3.PutToTheSword] = 2
    };

    public static readonly IReadOnlyCollection<Wildling> WildlingsFull = new[] {
        Wildling.AKingBeyondTheWall,
        Wildling.CrowKillers,
        Wildling.MammothRiders,
        Wildling.MassingOnTheMilkwater,
        Wildling.PreemptiveRaid,
        Wildling.RattleshirtsRaiders,
        Wildling.SilenceAtTheWall,
        Wildling.SkinchangerScout,
        Wildling.TheHordeDescends
    };

    public int Phase1Count { get; }
    public int Phase2Count { get; }
    public int Phase3Count { get; }
    public int WildlingsCount { get; }

    public WesterosDeck(WesterosStats? stats) : this(
        Phase1Full.ToDictionary(pair => pair.Key, pair => pair.Value),
        Phase2Full.ToDictionary(pair => pair.Key, pair => pair.Value),
        Phase3Full.ToDictionary(pair => pair.Key, pair => pair.Value),
        WildlingsFull.ToDictionary(wildlings => wildlings, _ => true),
        stats?.Turn ?? 0)
    {
        if (stats != null)
        {
            foreach (var events in stats.Phase1)
                foreach (var @event in events)
                {
                    if (@event != WesterosPhase1.WinterIsComing)
                        Phase1[@event]--;
                    else
                        foreach (var (key, value) in Phase1Full)
                            Phase1[key] = value;
                }

            foreach (var events in stats.Phase2)
                foreach (var @event in events)
                {
                    if (@event != WesterosPhase2.WinterIsComing)
                        Phase2[@event]--;
                    else
                        foreach (var (key, value) in Phase2Full)
                            Phase2[key] = value;
                }

            foreach (var events in stats.Phase3)
                foreach (var @event in events)
                    Phase3[@event]--;

            foreach (var wildlings in stats.Wildlings)
                foreach (var wildling in wildlings)
                    Wildlings[wildling] = false;
        }

        Phase1Count = Phase1.Sum(pair => pair.Value);
        Phase2Count = Phase2.Sum(pair => pair.Value);
        Phase3Count = Phase3.Sum(pair => pair.Value);
        WildlingsCount = Wildlings.Count(pair => pair.Value);
    }
}

public record WesterosProbabilities(
    KeyValuePair<WesterosPhase1, double>[] Phase1,
    KeyValuePair<WesterosPhase2, double>[] Phase2,
    KeyValuePair<WesterosPhase3, double>[] Phase3,
    KeyValuePair<Wildling, bool>[] Wildlings,
    int Turn)
{
    public WesterosProbabilities(WesterosStats? stats) : this(new WesterosDeck(stats)) { }

    public WesterosProbabilities(WesterosDeck? deck) : this(
        GetPhase1Probabilities(deck),
        GetPhase2Probabilities(deck),
        GetPhase3Probabilities(deck),
        GetWildling(deck),
        deck?.Turn ?? 0)
    { }

    private static KeyValuePair<WesterosPhase1, double>[] GetPhase1Probabilities(WesterosDeck? deck) =>
        deck == null
            ? Array.Empty<KeyValuePair<WesterosPhase1, double>>()
            : deck.Phase1.Select(pair => new KeyValuePair<WesterosPhase1, double>(pair.Key,
                (double)pair.Value / deck.Phase1Count
                + (double)deck.Phase1[WesterosPhase1.WinterIsComing] / deck.Phase1Count * WesterosDeck.Phase1Full[pair.Key] / 9))
                .OrderBy(pair => -pair.Value)
                .ToArray();

    private static KeyValuePair<WesterosPhase2, double>[] GetPhase2Probabilities(WesterosDeck? deck) =>
        deck == null
            ? Array.Empty<KeyValuePair<WesterosPhase2, double>>()
            : deck.Phase2.Select(pair => new KeyValuePair<WesterosPhase2, double>(pair.Key,
                (double)pair.Value / deck.Phase2Count
                + (double)deck.Phase2[WesterosPhase2.WinterIsComing] / deck.Phase2Count * WesterosDeck.Phase2Full[pair.Key] / 9))
                .OrderBy(pair => -pair.Value)
                .ToArray();

    private static KeyValuePair<WesterosPhase3, double>[] GetPhase3Probabilities(WesterosDeck? deck) =>
        deck == null
            ? Array.Empty<KeyValuePair<WesterosPhase3, double>>()
            : deck.Phase3.Select(pair => new KeyValuePair<WesterosPhase3, double>(pair.Key,
                (double)pair.Value / deck.Phase3Count))
                .OrderBy(pair => -pair.Value)
                .ToArray();

    private static KeyValuePair<Wildling, bool>[] GetWildling(WesterosDeck? deck) =>
        deck == null
            ? Array.Empty<KeyValuePair<Wildling, bool>>()
            : deck.Wildlings.Select(pair => new KeyValuePair<Wildling, bool>(pair.Key, pair.Value))
                .OrderBy(pair => !pair.Value)
                .ToArray();
}