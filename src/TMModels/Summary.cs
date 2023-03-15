using System.Text.Json.Serialization;

namespace TMModels;

public record Summary(
    string LeagueId,
    string LeagueName,
    SummaryDivision[] Divisions)
{
    public static Summary operator +(Summary summary1, Summary summary2)
    {
        var divisions = summary1.Divisions
            .Concat(summary2.Divisions)
            .GroupBy(division => (division.DivisionId, division.DivisionName))
            .Select(grouping => grouping.Aggregate(
                new SummaryDivision(grouping.Key.DivisionId, grouping.Key.DivisionName),
                (divisionS1, divisionS2) => divisionS1 + divisionS2))
            .ToArray();

        return summary1 with { Divisions = divisions };
    }

    public void Sort(Tiebreaker[] tiebreakers)
    {
        foreach (var division in Divisions)
            division.Sort(tiebreakers);
    }
}

public record SummaryDivision(
    string DivisionId,
    string DivisionName,
    SummaryPlayerScore[] Players)
{
    [JsonConstructor]
    public SummaryDivision(string divisionId, string divisionName) :
        this(divisionId, divisionName, Array.Empty<SummaryPlayerScore>())
    { }

    public static SummaryDivision operator +(SummaryDivision division1, SummaryDivision division2)
    {
        var players = division1.Players
            .Concat(division2.Players)
            .GroupBy(playerScore => playerScore.Player)
            .Select(grouping => grouping.Aggregate(
                new SummaryPlayerScore(grouping.Key),
                (player1, player2) => player1 + player2))
            .ToArray();

        return division1 with { Players = players };
    }

    public void Sort(Tiebreaker[] tiebreakers) =>
        Array.Sort(Players, (player1, player2) =>
            -player1.Compare(player2, tiebreakers));
}

public record SummaryPlayerScore(
    string Player,
    SummaryScore Best,
    SummaryScore Total,
    int Seasons = 1)
{
    [JsonConstructor]
    public SummaryPlayerScore(string player) :
        this(player, new SummaryScore(), new SummaryScore(), 0)
    { }

    public static SummaryPlayerScore operator +(SummaryPlayerScore player1, SummaryPlayerScore player2) =>
        player1 with
        {
            Best = SummaryScore.Max(player1.Best, player2.Best),
            Total = player1.Total + player2.Total,
            Seasons = player1.Seasons + player2.Seasons
        };

    public int Compare(SummaryPlayerScore other, IEnumerable<Tiebreaker> tiebreakers)
    {
        if (Best.TotalPoints != other.Best.TotalPoints)
            return Best.TotalPoints.CompareTo(other.Best.TotalPoints);

        foreach (var tiebreaker in tiebreakers)
        {
            switch (tiebreaker)
            {
                case Tiebreaker.Wins:
                    if (Best.Wins.CompareTo(other.Best.Wins) == 0)
                        continue;
                    return Best.Wins.CompareTo(other.Best.Wins);

                case Tiebreaker.Penalties:
                    if (Best.PenaltiesPoints.CompareTo(other.Best.PenaltiesPoints) == 0)
                        continue;
                    return other.Best.PenaltiesPoints.CompareTo(Best.PenaltiesPoints);

                case Tiebreaker.Cla:
                    if (Best.Cla.CompareTo(other.Best.Cla) == 0)
                        continue;
                    return Best.Cla.CompareTo(other.Best.Cla);

                case Tiebreaker.Supplies:
                    if (Best.Supplies.CompareTo(other.Best.Supplies) == 0)
                        continue;
                    return Best.Supplies.CompareTo(other.Best.Supplies);

                case Tiebreaker.PowerTokens:
                    if (Best.PowerTokens.CompareTo(other.Best.PowerTokens) == 0)
                        continue;
                    return Best.PowerTokens.CompareTo(other.Best.PowerTokens);

                case Tiebreaker.MinutesPerMove:
                    if (Best.MinutesPerMove == null ||
                        other.Best.MinutesPerMove == null ||
                        Best.MinutesPerMove.Value.CompareTo(other.Best.MinutesPerMove) == 0)
                        continue;
                    return Best.MinutesPerMove.Value.CompareTo(other.Best.MinutesPerMove);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return 0;
    }
}

public record SummaryScore(
    double TotalPoints,
    int Wins,
    int Cla,
    int Supplies,
    int PowerTokens,
    double? MinutesPerMove,
    int Moves,
    SummaryHouseScore[] Houses,
    double PenaltiesPoints,
    int? Position,
    Stats Stats)
{
    public Stats Stats { get; } = Stats ?? new Stats();

    public SummaryScore() :
        this(0, 0, 0, 0, 0, null, 0, Array.Empty<SummaryHouseScore>(), 0, null, new Stats())
    { }

    public static SummaryScore Max(SummaryScore score1, SummaryScore score2) => new(
        Math.Max(score1.TotalPoints, score2.TotalPoints),
        Math.Max(score1.Wins, score2.Wins),
        Math.Max(score1.Cla, score2.Cla),
        Math.Max(score1.Supplies, score2.Supplies),
        Math.Max(score1.PowerTokens, score2.PowerTokens),
        score1.MinutesPerMove != null && score2.MinutesPerMove != null ?
            Math.Min(score1.MinutesPerMove.Value, score2.MinutesPerMove.Value) :
            score1.MinutesPerMove ?? score2.MinutesPerMove,
        Math.Max(score1.Moves, score2.Moves),
        score1.Houses
            .Concat(score2.Houses)
            .GroupBy(houseScore => houseScore.House)
            .Select(grouping => grouping.Aggregate(
                new SummaryHouseScore(grouping.Key),
                SummaryHouseScore.Max))
            .ToArray(),
        Math.Max(score1.PenaltiesPoints, score2.PenaltiesPoints),
        score1.Position != null && score2.Position != null ?
            Math.Min(score1.Position.Value, score2.Position.Value) :
            score1.Position ?? score2.Position,
        Stats.Max(score1.Stats, score2.Stats));

    public static SummaryScore operator +(SummaryScore score1, SummaryScore score2) => new(
        score1.TotalPoints + score2.TotalPoints,
        score1.Wins + score2.Wins,
        score1.Cla + score2.Cla,
        score1.Supplies + score2.Supplies,
        score1.PowerTokens + score2.PowerTokens,
        score1.MinutesPerMove != null && score2.MinutesPerMove != null ?
            (score1.MinutesPerMove * score1.Moves + score2.MinutesPerMove * score2.Moves) / (score1.Moves + score2.Moves) :
            score1.MinutesPerMove ?? score2.MinutesPerMove,
        score1.Moves + score2.Moves,
        score1.Houses
            .Concat(score2.Houses)
            .GroupBy(houseScore => houseScore.House)
            .Select(grouping => grouping.Aggregate(
                new SummaryHouseScore(grouping.Key),
                (houseScore1, houseScore2) => houseScore1 + houseScore2))
            .ToArray(),
        score1.PenaltiesPoints + score2.PenaltiesPoints,
        score1.Position != null && score2.Position != null ?
            score1.Position + score2.Position :
            score1.Position ?? score2.Position,
        score1.Stats + score2.Stats);
}

public record SummaryHouseScore(
    House House,
    double Points = 0)
{
    public static SummaryHouseScore Max(SummaryHouseScore score1, SummaryHouseScore score2) =>
        score1 with { Points = Math.Max(score1.Points, score2.Points) };

    public static SummaryHouseScore operator +(SummaryHouseScore score1, SummaryHouseScore score2) =>
        score1 with { Points = score1.Points + score2.Points };
}