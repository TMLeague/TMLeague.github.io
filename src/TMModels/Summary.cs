using System.Text.Json.Serialization;

namespace TMModels;

public record Summary(
    [property: JsonPropertyName("leagueId")] string LeagueId,
    [property: JsonPropertyName("leagueName")] string LeagueName,
    [property: JsonPropertyName("divisions")] SummaryDivision[] Divisions)
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
    [property: JsonPropertyName("divisionId")] string DivisionId,
    [property: JsonPropertyName("divisionName")] string DivisionName,
    [property: JsonPropertyName("players")] SummaryPlayerScore[] Players)
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
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("best")] SummaryScore Best,
    [property: JsonPropertyName("total")] SummaryScore Total,
    [property: JsonPropertyName("seasons")] ushort Seasons = 1)
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
            Seasons = (ushort)(player1.Seasons + player2.Seasons)
        };

    public int Compare(SummaryPlayerScore other, IEnumerable<Tiebreaker> tiebreakers)
    {
        if (Best.TotalPoints != other.Best.TotalPoints)
            return Best.TotalPoints - other.Best.TotalPoints;

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
                    if (Best.MinutesPerMove.CompareTo(other.Best.MinutesPerMove) == 0)
                        continue;
                    return Best.MinutesPerMove.CompareTo(other.Best.MinutesPerMove);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return 0;
    }
}

public record SummaryScore(
    [property: JsonPropertyName("totalPoints")] int TotalPoints,
    [property: JsonPropertyName("wins")] uint Wins,
    [property: JsonPropertyName("cla")] uint Cla,
    [property: JsonPropertyName("supplies")] uint Supplies,
    [property: JsonPropertyName("powerTokens")] uint PowerTokens,
    [property: JsonPropertyName("minutesPerMove")] double MinutesPerMove,
    [property: JsonPropertyName("moves")] uint Moves,
    [property: JsonPropertyName("houses")] SummaryHouseScore[] Houses,
    [property: JsonPropertyName("penaltiesPoints")] uint PenaltiesPoints,
    [property: JsonPropertyName("position")] uint Position)
{
    public SummaryScore() :
        this(0, 0, 0, 0, 0, double.MaxValue, 0, Array.Empty<SummaryHouseScore>(), 0, uint.MaxValue)
    { }

    public static SummaryScore Max(SummaryScore score1, SummaryScore score2) => new(
        Math.Max(score1.TotalPoints, score2.TotalPoints),
        Math.Max(score1.Wins, score2.Wins),
        Math.Max(score1.Cla, score2.Cla),
        Math.Max(score1.Supplies, score2.Supplies),
        Math.Max(score1.PowerTokens, score2.PowerTokens),
        Math.Min(score1.MinutesPerMove, score2.MinutesPerMove),
        Math.Max(score1.Moves, score2.Moves),
        score1.Houses
            .Concat(score2.Houses)
            .GroupBy(houseScore => houseScore.House)
            .Select(grouping => grouping.Aggregate(
                new SummaryHouseScore(grouping.Key),
                SummaryHouseScore.Max))
            .ToArray(),
        Math.Min(score1.PenaltiesPoints, score2.PenaltiesPoints),
        Math.Min(score1.Position, score2.Position));

    public static SummaryScore operator +(SummaryScore score1, SummaryScore score2) => new(
        score1.TotalPoints + score2.TotalPoints,
        score1.Wins + score2.Wins,
        score1.Cla + score2.Cla,
        score1.Supplies + score2.Supplies,
        score1.PowerTokens + score2.PowerTokens,
        (score1.MinutesPerMove * score1.Moves + score2.MinutesPerMove * score2.Moves) / (score1.Moves + score2.Moves),
        score1.Moves + score2.Moves,
        score1.Houses
            .Concat(score2.Houses)
            .GroupBy(houseScore => houseScore.House)
            .Select(grouping => grouping.Aggregate(
                new SummaryHouseScore(grouping.Key),
                (houseScore1, houseScore2) => houseScore1 + houseScore2))
            .ToArray(),
        score1.PenaltiesPoints + score2.PenaltiesPoints,
        score1.Position + score2.Position
    );
}

public record SummaryHouseScore(
    [property: JsonPropertyName("house")] House House,
    [property: JsonPropertyName("points")] uint Points = 0)
{
    public static SummaryHouseScore Max(SummaryHouseScore score1, SummaryHouseScore score2) =>
        score1 with { Points = Math.Max(score1.Points, score2.Points) };

    public static SummaryHouseScore operator +(SummaryHouseScore score1, SummaryHouseScore score2) =>
        score1 with { Points = score1.Points + score2.Points };
}