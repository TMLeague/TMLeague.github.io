using System.Text.Json.Serialization;

namespace TMModels;

public record Summary(
    [property: JsonPropertyName("league")] string League,
    [property: JsonPropertyName("divisions")] SummaryDivision[] Divisions)
{
    public static Summary operator +(Summary summary1, Summary summary2)
    {
        var divisions = summary1.Divisions
            .Concat(summary2.Divisions)
            .GroupBy(division => division.Division)
            .Select(grouping => grouping.Aggregate(
                new SummaryDivision(grouping.Key),
                (divisionS1, divisionS2) => divisionS1 + divisionS2))
            .ToArray();

        return summary1 with { Divisions = divisions };
    }
}

public record SummaryDivision(
    [property: JsonPropertyName("division")] string Division,
    [property: JsonPropertyName("players")] SummaryPlayerScore[] Players)
{
    public SummaryDivision(string division) :
        this(division, Array.Empty<SummaryPlayerScore>())
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
}

public record SummaryPlayerScore(
    [property: JsonPropertyName("player")] string Player,
    [property: JsonPropertyName("best")] SummaryScore Best,
    [property: JsonPropertyName("total")] SummaryScore Total,
    [property: JsonPropertyName("seasons")] ushort Seasons = 1)
{
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
    [property: JsonPropertyName("penaltiesPoints")] uint PenaltiesPoints)
{
    public SummaryScore() :
        this(0, 0, 0, 0, 0, double.MaxValue, 0, Array.Empty<SummaryHouseScore>(), 0)
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
        Math.Min(score1.PenaltiesPoints, score2.PenaltiesPoints));

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
        score1.PenaltiesPoints + score2.PenaltiesPoints
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