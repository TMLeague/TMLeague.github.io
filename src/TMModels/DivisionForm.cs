using TMModels.Extensions;

namespace TMModels;

public class DivisionForm
{
    public string? League { get; set; }
    public string? Season { get; set; }
    public string? Division { get; set; }
    public string? Password { get; set; }
    public string? Contact { get; set; }
    public string? JudgeName { get; set; }
    public string? MessageSubject { get; set; }
    public string? MessageBody { get; set; }
    public string? Player1 { get; set; }
    public string? Player2 { get; set; }
    public string? Player3 { get; set; }
    public string? Player4 { get; set; }
    public string? Player5 { get; set; }
    public string? Player6 { get; set; }
    public string? Player7 { get; set; }
    public string? Player8 { get; set; }
    public string? Player9 { get; set; }
    public string? Player10 { get; set; }
    public string[] Players => new[] { Player1, Player2, Player3, Player4, Player5, Player6, Player7, Player8, Player9, Player10 }
        .Where(p => !string.IsNullOrWhiteSpace(p))
        .Select(s => s!).ToArray();

    public DivisionFormRandomOptions RandomOptions { get; set; } = new();
}

public class DivisionFormRandomOptions
{
    public bool UseRandomDraft { get; set; }

    public TimeSpan Timeout { get; set; } = TimeSpan.Zero;
    public string TimeoutRaw
    {
        get => Timeout.ToString();
        set => Timeout = TimeSpan.TryParse(value, out var timeSpan) ? timeSpan : TimeSpan.Zero;
    }

    public DraftScoreWeights Weights { get; set; } = new();
}

public class DraftScoreWeights
{
    public double NeighborMin { get; set; } = 1;
    public double NeighborMax { get; set; } = -1;
    public double NeighborStd { get; set; } = -1;
    public double EnemyMin { get; set; } = 1;
    public double EnemyMax { get; set; } = -1;
    public double EnemyStd { get; set; } = -1;
    public double ProximityMin { get; set; } = 1;
    public double ProximityMax { get; set; } = -1;
    public double ProximityStd { get; set; } = -1;
}

public record DivisionDraft(List<PlayerDraft> Draft, bool IsRandom)
{
    public int Players => Draft.Count;
    public int Games => Draft.FirstOrDefault()?.Games.Length ?? 0;

    private IEnumerable<PlayerDraftStat> AllStats => Draft
        .SelectMany(draft => draft.Stats)
        .OfType<PlayerDraftStat>();

    public int NeighborMin => AllStats.Select(stat => stat.Neighbor).Min();
    public int NeighborMax => AllStats.Select(stat => stat.Neighbor).Max();
    public double NeighborStd => AllStats.Select(stat => stat.Neighbor).ToArray().Std();
    public int EnemyMin => AllStats.Select(stat => stat.Enemy).Min();
    public int EnemyMax => AllStats.Select(stat => stat.Enemy).Max();
    public double EnemyStd => AllStats.Select(stat => stat.Enemy).ToArray().Std();
    public double ProximityMin => AllStats.Select(stat => stat.Proximity).Min();
    public double ProximityMax => AllStats.Select(stat => stat.Proximity).Max();
    public double ProximityStd => AllStats.Select(stat => stat.Proximity).ToArray().Std();

    public double GetScore(DraftScoreWeights weights) =>
        NeighborMin * weights.NeighborMin +
        NeighborMax * weights.NeighborMax +
        NeighborStd * weights.NeighborStd +
        EnemyMin * weights.EnemyMin +
        EnemyMax * weights.EnemyMax +
        EnemyStd * weights.EnemyStd +
        ProximityMin * weights.ProximityMin +
        ProximityMax * weights.ProximityMax +
        ProximityStd * weights.ProximityStd;
}

public record PlayerDraft(
    string Name,
    House[] Games,
    string MessageSubject,
    string MessageBody,
    PlayerDraftStats Stats);

public class PlayerDraftStats : List<PlayerDraftStat?>
{
    public int NeighborMin => this.Min(stat => stat?.Neighbor ?? int.MaxValue);
    public int NeighborMax => this.Max(stat => stat?.Neighbor ?? 0);
    public int EnemyMin => this.Min(stat => stat?.Enemy ?? int.MaxValue);
    public int EnemyMax => this.Max(stat => stat?.Enemy ?? 0);
    public double ProximityMin => this.Min(stat => stat?.Proximity ?? int.MaxValue);
    public double ProximityMax => this.Max(stat => stat?.Proximity ?? 0);

    public PlayerDraftStats() { }
    public PlayerDraftStats(IEnumerable<PlayerDraftStat?> collection) : base(collection) { }
}

/// <summary>
/// Player vs player stats
/// </summary>
/// <param name="Player">Player name</param>
/// <param name="Neighbor">Number of games in which player is a neighbor</param>
/// <param name="Enemy">Number of games in which player is an enemy</param>
/// <param name="NeighborPairs">Number of pairs of games where players have reversed houses neighboring each other</param>
/// <param name="EnemyPairs">Number of pairs of games where players have reversed houses</param>
public record PlayerDraftStat(string Player, int Neighbor, int Enemy, double Proximity, int NeighborPairs, int EnemyPairs);

public record PlayerHouseGames(int Baratheon, int Lannister, int Stark, int Tyrell, int Greyjoy, int Martell, int Arryn);