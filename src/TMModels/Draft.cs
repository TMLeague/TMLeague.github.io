using TMModels.Extensions;

namespace TMModels;

public record Draft(
    string Name,
    string[][] Table)
{
    public string Serialize() =>
        string.Join(Environment.NewLine, Table.Select(row => string.Join('\t', row)));
}

public record DraftScore(string Id,
    ScoreData Neighbor, ScoreData Game, ScoreData Proximity, ScoreData NeighborPairs, ScoreData GamePairs, ScoreData Allies, ScoreData Enemies, ScoreData Relations)
{
    private const double Precision = 0.001;

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

    public bool IsDominating(DraftScore other, QualityMeasures measures) => measures.List.All(measureOptions =>
        !measureOptions.Enabled ||
        measureOptions.GetScore(this).IsDominating(measureOptions.GetScore(other)));

    public bool IsMeetingConstraints(QualityMeasures measures) => measures.List.All(measureOptions =>
        !measureOptions.Enabled ||
        ((measureOptions.Min == null || measureOptions.GetScore(this).Min + Precision >= measureOptions.Min.Value) &&
         (measureOptions.Max == null || measureOptions.GetScore(this).Max <= measureOptions.Max + Precision) &&
         (measureOptions.Std == null || measureOptions.GetScore(this).Std <= measureOptions.Std + Precision)));

    public bool IsEqual(DraftScore other, QualityMeasures measures) => measures.List.All(measureOptions =>
        !measureOptions.Enabled ||
        measureOptions.GetScore(this).IsEqual(measureOptions.GetScore(other)));
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
    public NeighborQualityMeasureOptions Neighbor { get; set; } = new();
    public GameQualityMeasureOptions Game { get; set; } = new();
    public ProximityQualityMeasureOptions Proximity { get; set; } = new();
    public NeighborPairsQualityMeasureOptions NeighborPairs { get; set; } = new();
    public GamePairsQualityMeasureOptions GamePairs { get; set; } = new();
    public AlliesQualityMeasureOptions Allies { get; set; } = new();
    public EnemiesQualityMeasureOptions Enemies { get; set; } = new();
    public RelationsQualityMeasureOptions Relations { get; set; } = new();

    public QualityMeasureOptions[] List { get; }

    public QualityMeasures()
    {
        List = new QualityMeasureOptions[] { Neighbor, Game, Proximity, NeighborPairs, GamePairs, Allies, Enemies, Relations };
    }
}

public class NeighborQualityMeasureOptions : QualityMeasureOptions
{
    public override string Name => "Neighbor";
    public override Func<DraftScore, ScoreData> GetScore => score => score.Neighbor;
}
public class GameQualityMeasureOptions : QualityMeasureOptions
{
    public override string Name => "Game";
    public override Func<DraftScore, ScoreData> GetScore => score => score.Game;
}
public class ProximityQualityMeasureOptions : QualityMeasureOptions
{
    public override string Name => "Proximity";
    public override Func<DraftScore, ScoreData> GetScore => score => score.Proximity;
}
public class NeighborPairsQualityMeasureOptions : QualityMeasureOptions
{
    public override string Name => "Neighbor pairs";
    public override Func<DraftScore, ScoreData> GetScore => score => score.NeighborPairs;
}
public class GamePairsQualityMeasureOptions : QualityMeasureOptions
{
    public override string Name => "Game pairs";
    public override Func<DraftScore, ScoreData> GetScore => score => score.GamePairs;
}
public class AlliesQualityMeasureOptions : QualityMeasureOptions
{
    public override string Name => "Allies";
    public override Func<DraftScore, ScoreData> GetScore => score => score.Allies;
}
public class EnemiesQualityMeasureOptions : QualityMeasureOptions
{
    public override string Name => "Enemies";
    public override Func<DraftScore, ScoreData> GetScore => score => score.Enemies;
}
public class RelationsQualityMeasureOptions : QualityMeasureOptions
{
    public override string Name => "Relations";
    public override Func<DraftScore, ScoreData> GetScore => score => score.Relations;
}

public abstract class QualityMeasureOptions
{
    public abstract string Name { get; }
    public abstract Func<DraftScore, ScoreData> GetScore { get; }
    public bool Enabled { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public double? Std { get; set; }
    public double? Weight { get; set; }
}