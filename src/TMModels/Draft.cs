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
    ScoreData Neighbor, ScoreData Game, ScoreData Proximity)
{
    public DraftScore(string id, PlayerDraftStat[] allStats) : this(
        id,
        new ScoreData(allStats, stat => stat.Neighbor),
        new ScoreData(allStats, stat => stat.Games),
        new ScoreData(allStats, stat => stat.Proximity))
    { }

    public bool IsDominating(DraftScore other, QualityMeasures measures)
    {
        if (measures.Neighbor && !Neighbor.IsDominating(other.Neighbor))
            return false;
        if (measures.Game && !Game.IsDominating(other.Game))
            return false;
        if (measures.Proximity && !Proximity.IsDominating(other.Proximity))
            return false;

        return true;
    }

    public bool IsEqual(DraftScore other, QualityMeasures measures) =>
        (!measures.Neighbor || Neighbor.IsEqual(other.Neighbor)) &&
        (!measures.Game || Game.IsEqual(other.Neighbor)) &&
        (!measures.Proximity || Proximity.IsEqual(other.Neighbor));
}

public record ScoreData(double Min, double Max, double Std)
{
    private const double Precision = 0.001;

    public ScoreData(PlayerDraftStat[] stats, Func<PlayerDraftStat, double> selector) : this(
        stats.Select(selector).Min(),
        stats.Select(selector).Max(),
        stats.Select(selector).ToArray().Std())
    { }

    public bool IsDominating(ScoreData other) =>
        Min + 0.001 >= other.Min &&
        Max <= other.Max + 0.001 &&
        Std <= other.Std + 0.001;

    public bool IsEqual(ScoreData other) =>
        Math.Abs(Min - other.Min) < Precision &&
        Math.Abs(Max - other.Max) < Precision &&
        Math.Abs(Std - other.Std) < Precision;
}

public class QualityMeasures
{
    public bool Neighbor { get; set; }
    public bool Game { get; set; }
    public bool Proximity { get; set; }
}