using TMModels.Extensions;

namespace TMModels;

public record Draft(
    string Name,
    string[][] Table)
{
    public string Serialize() =>
        string.Join(Environment.NewLine, Table.Select(row => string.Join("\t", row)));
}

public record DraftScore(string Id,
    int NeighborMin, int NeighborMax, double NeighborStd,
    int EnemyMin, int EnemyMax, double EnemyStd)
{
    public DraftScore(string id, PlayerDraftStat[] allStats) : this(
        id,
        allStats.Select(stat => stat.Neighbor).Min(),
        allStats.Select(stat => stat.Neighbor).Max(),
        allStats.Select(stat => stat.Neighbor).ToArray().Std(),
        allStats.Select(stat => stat.Enemy).Min(),
        allStats.Select(stat => stat.Enemy).Max(),
        allStats.Select(stat => stat.Enemy).ToArray().Std())
    { }

    public bool IsDominating(DraftScore other) =>
        NeighborMin >= other.NeighborMin &&
        NeighborMax <= other.NeighborMax &&
        NeighborStd <= other.NeighborStd + 0.001 &&
        EnemyMin >= other.EnemyMin &&
        EnemyMax <= other.EnemyMax &&
        EnemyStd <= other.EnemyStd + 0.001;

    public bool IsEqual(DraftScore other) => NeighborMin == other.NeighborMin &&
                                             NeighborMax == other.NeighborMax &&
                                             Math.Abs(NeighborStd - other.NeighborStd) < 0.001 &&
                                             EnemyMin == other.EnemyMin &&
                                             EnemyMax == other.EnemyMax &&
                                             Math.Abs(EnemyStd - other.EnemyStd) < 0.001;
}