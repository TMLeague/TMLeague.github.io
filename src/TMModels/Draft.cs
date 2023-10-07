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
    ScoreData Neighbor, ScoreData Game, ScoreData Proximity, ScoreData NeighborPairs, ScoreData GamePairs, ScoreData Allies, ScoreData Enemies, ScoreData Relations)
{
    public DraftScore(string id, PlayerDraftStat[] allStats) : this(
        id,
        new ScoreData(allStats, stat => stat.Neighbor),
        new ScoreData(allStats, stat => stat.Games),
        new ScoreData(allStats, stat => stat.Proximity),
        new ScoreData(allStats, stat => stat.NeighborPairs),
        new ScoreData(allStats, stat => stat.GamesPairs),
        new ScoreData(allStats, stat => stat.Allies),
        new ScoreData(allStats, stat => stat.Enemies),
        new ScoreData(allStats, stat => stat.Relations))
    { }

    public bool IsDominating(DraftScore other, QualityMeasures measures)
    {
        if (measures.Neighbor && !Neighbor.IsDominating(other.Neighbor))
            return false;
        if (measures.Game && !Game.IsDominating(other.Game))
            return false;
        if (measures.Proximity && !Proximity.IsDominating(other.Proximity))
            return false;
        if (measures.NeighborPairs && !NeighborPairs.IsDominating(other.NeighborPairs))
            return false;
        if (measures.GamePairs && !GamePairs.IsDominating(other.GamePairs))
            return false;
        if (measures.Allies && !Allies.IsDominating(other.Allies))
            return false;
        if (measures.Enemies && !Enemies.IsDominating(other.Enemies))
            return false;
        if (measures.Relations && !Relations.IsDominating(other.Relations))
            return false;

        return true;
    }

    public bool IsEqual(DraftScore other, QualityMeasures measures) =>
        (!measures.Neighbor || Neighbor.IsEqual(other.Neighbor)) &&
        (!measures.Game || Game.IsEqual(other.Neighbor)) &&
        (!measures.Proximity || Proximity.IsEqual(other.Neighbor)) &&
        (!measures.NeighborPairs || NeighborPairs.IsEqual(other.NeighborPairs)) &&
        (!measures.GamePairs || GamePairs.IsEqual(other.GamePairs)) &&
        (!measures.Allies || Allies.IsEqual(other.Allies)) &&
        (!measures.Enemies || Enemies.IsEqual(other.Enemies)) &&
        (!measures.Relations || Relations.IsEqual(other.Relations));
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
    public bool NeighborPairs { get; set; }
    public bool GamePairs { get; set; }
    public bool Allies { get; set; }
    public bool Enemies { get; set; }
    public bool Relations { get; set; }
}