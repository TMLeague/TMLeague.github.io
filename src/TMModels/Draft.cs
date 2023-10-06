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
    ScoreData Neighbor, ScoreData Enemy, ScoreData Proximity)
{
    public DraftScore(string id, PlayerDraftStat[] allStats) : this(
        id,
        new ScoreData(allStats, stat => stat.Neighbor),
        new ScoreData(allStats, stat => stat.Enemy),
        new ScoreData(allStats, stat => stat.Proximity))
    { }

    public bool IsDominating(DraftScore other) =>
        Neighbor.Min >= other.Neighbor.Min &&
        Neighbor.Max <= other.Neighbor.Max &&
        Neighbor.Std <= other.Neighbor.Std + 0.001 &&
        Enemy.Min >= other.Enemy.Min &&
        Enemy.Max <= other.Enemy.Max &&
        Enemy.Std <= other.Enemy.Std + 0.001 &&
        Proximity.Min >= other.Proximity.Min &&
        Proximity.Max <= other.Proximity.Max &&
        Proximity.Std <= other.Proximity.Std + 0.001;

    public bool IsEqual(DraftScore other) =>
        Neighbor.IsEqual(other.Neighbor) &&
        Enemy.IsEqual(other.Neighbor) &&
        Proximity.IsEqual(other.Neighbor);
}

public record ScoreData(double Min, double Max, double Std)
{
    private const double Precision = 0.001;

    public ScoreData(PlayerDraftStat[] stats, Func<PlayerDraftStat, double> selector) : this(
        stats.Select(selector).Min(),
        stats.Select(selector).Max(),
        stats.Select(selector).ToArray().Std())
    { }

    public bool IsEqual(ScoreData other)
    {
        return Math.Abs(Min - other.Min) < Precision &&
               Math.Abs(Max - other.Max) < Precision &&
               Math.Abs(Std - other.Std) < Precision;
    }
}