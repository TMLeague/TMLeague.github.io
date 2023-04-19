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
    SummaryPlayerScore[] Players,
    SummaryHouseScore[] Houses)
{
    [JsonConstructor]
    public SummaryDivision(string divisionId, string divisionName) :
        this(divisionId, divisionName,
            Array.Empty<SummaryPlayerScore>(),
            Array.Empty<SummaryHouseScore>())
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
        var houses = division1.Houses
            .Concat(division2.Houses)
            .GroupBy(houseScore => houseScore.House)
            .Select(grouping => grouping.Aggregate(
                new SummaryHouseScore(grouping.Key),
                (house1, house2) => house1 + house2))
            .ToArray();

        return division1 with
        {
            Players = players,
            Houses = houses
        };
    }

    public void Sort(Tiebreaker[] tiebreakers)
    {
        Array.Sort(Players, (player1, player2) =>
            -player1.Compare(player2, tiebreakers));
        Array.Sort(Houses, (house1, house2) =>
            -house1.Compare(house2, tiebreakers));
    }
}

public record SummaryPlayerScore(
    string Player,
    SummaryPlayerScoreDetails Best,
    SummaryPlayerScoreDetails Total,
    int Seasons = 1)
{
    private Score BestScore => new(Best.TotalPoints, Best.Wins, Best.PenaltiesPoints,
        Best.Cla, Best.Supplies, Best.PowerTokens, Best.MinutesPerMove);

    [JsonConstructor]
    public SummaryPlayerScore(string player) :
        this(player, new SummaryPlayerScoreDetails(), new SummaryPlayerScoreDetails(), 0)
    { }

    public static SummaryPlayerScore operator +(SummaryPlayerScore player1, SummaryPlayerScore player2) =>
        player1 with
        {
            Best = SummaryPlayerScoreDetails.Max(player1.Best, player2.Best),
            Total = player1.Total + player2.Total,
            Seasons = player1.Seasons + player2.Seasons
        };

    public int Compare(SummaryPlayerScore other, IEnumerable<Tiebreaker> tiebreakers) =>
        BestScore.Compare(other.BestScore, tiebreakers);
}

public record SummaryPlayerScoreDetails(
    double TotalPoints,
    int Wins,
    int Cla,
    int Supplies,
    int PowerTokens,
    double? MinutesPerMove,
    int Moves,
    HousePoints[] Houses,
    double PenaltiesPoints,
    int? Position,
    Stats? Stats)
{
    public SummaryPlayerScoreDetails() :
        this(0, 0, 0, 0, 0, null, 0, Array.Empty<HousePoints>(), 0, null, null)
    { }

    public static SummaryPlayerScoreDetails Max(SummaryPlayerScoreDetails score1, SummaryPlayerScoreDetails score2) => new(
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
                new HousePoints(grouping.Key),
                HousePoints.Max))
            .ToArray(),
        Math.Max(score1.PenaltiesPoints, score2.PenaltiesPoints),
        score1.Position != null && score2.Position != null ?
            Math.Min(score1.Position.Value, score2.Position.Value) :
            score1.Position ?? score2.Position,
        score1.Stats != null && score2.Stats != null ?
            Stats.Max(score1.Stats, score2.Stats) :
            score1.Stats ?? score2.Stats);

    public static SummaryPlayerScoreDetails operator +(SummaryPlayerScoreDetails score1, SummaryPlayerScoreDetails score2) => new(
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
                new HousePoints(grouping.Key),
                (houseScore1, houseScore2) => houseScore1 + houseScore2))
            .ToArray(),
        score1.PenaltiesPoints + score2.PenaltiesPoints,
        score1.Position != null && score2.Position != null ?
            score1.Position + score2.Position :
            score1.Position ?? score2.Position,
        score1.Stats != null && score2.Stats != null ?
            score1.Stats + score2.Stats :
            score1.Stats ?? score2.Stats);
}

public record HousePoints(
    House House,
    double Points = 0)
{
    public static HousePoints Max(HousePoints score1, HousePoints score2) =>
        score1 with { Points = Math.Max(score1.Points, score2.Points) };

    public static HousePoints operator +(HousePoints score1, HousePoints score2) =>
        score1 with { Points = score1.Points + score2.Points };
}

public record SummaryHouseScore(
    House House,
    SummaryHouseScoreDetails Best,
    SummaryHouseScoreDetails Total,
    int Games = 1)
{
    private Score BestScore => new(Best.Points, Best.Wins, 0,
        Best.Cla, Best.Supplies, Best.PowerTokens, null);

    [JsonConstructor]
    public SummaryHouseScore(House house) :
        this(house, new SummaryHouseScoreDetails(), new SummaryHouseScoreDetails(), 0)
    { }

    public int Compare(SummaryHouseScore other, IEnumerable<Tiebreaker> tiebreakers) =>
        BestScore.Compare(other.BestScore, tiebreakers);

    public static SummaryHouseScore operator +(SummaryHouseScore house1, SummaryHouseScore house2) =>
        house1 with
        {
            Best = SummaryHouseScoreDetails.Max(house1.Best, house2.Best),
            Total = house1.Total + house2.Total,
            Games = house1.Games + house2.Games
        };
}

public record SummaryHouseScoreDetails(
    double Points,
    int Wins,
    int Cla,
    int Supplies,
    int PowerTokens,
    int Moves,
    Stats? Stats)
{
    public SummaryHouseScoreDetails() :
        this(0, 0, 0, 0, 0, 0, null)
    { }

    public static SummaryHouseScoreDetails Max(SummaryHouseScoreDetails score1, SummaryHouseScoreDetails score2) => new(
        Math.Max(score1.Points, score2.Points),
        Math.Max(score1.Wins, score2.Wins),
        Math.Max(score1.Cla, score2.Cla),
        Math.Max(score1.Supplies, score2.Supplies),
        Math.Max(score1.PowerTokens, score2.PowerTokens),
        Math.Max(score1.Moves, score2.Moves),
        score1.Stats != null && score2.Stats != null ?
            Stats.Max(score1.Stats, score2.Stats) :
            score1.Stats ?? score2.Stats);

    public static SummaryHouseScoreDetails operator +(SummaryHouseScoreDetails score1, SummaryHouseScoreDetails score2) => new(
        score1.Points + score2.Points,
        score1.Wins + score2.Wins,
        score1.Cla + score2.Cla,
        score1.Supplies + score2.Supplies,
        score1.PowerTokens + score2.PowerTokens,
        score1.Moves + score2.Moves,
        score1.Stats != null && score2.Stats != null ?
            score1.Stats + score2.Stats :
            score1.Stats ?? score2.Stats);
}

public record Score(double Points, double Wins, double Penalties, double Cla, double Supplies, double PowerTokens, double? MinutesPerMove)
{
    public int Compare(Score other, IEnumerable<Tiebreaker> tiebreakers)
    {
        if (Math.Abs(Points - other.Points) > 0.001)
            return Points.CompareTo(other.Points);

        foreach (var tiebreaker in tiebreakers)
        {
            switch (tiebreaker)
            {
                case Tiebreaker.Wins:
                    if (Wins.CompareTo(other.Wins) == 0)
                        continue;
                    return Wins.CompareTo(other.Wins);

                case Tiebreaker.Penalties:
                    if (Penalties.CompareTo(other.Penalties) == 0)
                        continue;
                    return other.Penalties.CompareTo(Penalties);

                case Tiebreaker.Cla:
                    if (Cla.CompareTo(other.Cla) == 0)
                        continue;
                    return Cla.CompareTo(other.Cla);

                case Tiebreaker.Supplies:
                    if (Supplies.CompareTo(other.Supplies) == 0)
                        continue;
                    return Supplies.CompareTo(other.Supplies);

                case Tiebreaker.PowerTokens:
                    if (PowerTokens.CompareTo(other.PowerTokens) == 0)
                        continue;
                    return PowerTokens.CompareTo(other.PowerTokens);

                case Tiebreaker.MinutesPerMove:
                    if (MinutesPerMove == null ||
                        other.MinutesPerMove == null ||
                        MinutesPerMove.Value.CompareTo(other.MinutesPerMove) == 0)
                        continue;
                    return MinutesPerMove.Value.CompareTo(other.MinutesPerMove);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return 0;
    }
}