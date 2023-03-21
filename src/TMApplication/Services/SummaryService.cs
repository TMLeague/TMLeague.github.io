using TMApplication.Providers;
using TMApplication.ViewModels;
using TMModels;

namespace TMApplication.Services;

public class SummaryService
{
    private readonly IDataProvider _dataProvider;

    public SummaryService(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<SummaryViewModel?> GetSummaryVm(string leagueId, string? divisionId = null, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);

        var summary = await _dataProvider.GetSummary(leagueId, cancellationToken);
        if (summary == null)
            return null;

        var division = divisionId == null
            ? summary.Divisions.Aggregate(
                new SummaryDivision(string.Empty, string.Empty),
                (division1, division2) => division1 + division2)
            : summary.Divisions.FirstOrDefault(summaryDivision =>
                summaryDivision.DivisionId == divisionId) ??
              new SummaryDivision(divisionId, string.Empty);

        Array.Sort(division.Players, (score1, score2) => -score1.Compare(score2, league?.Scoring?.Tiebreakers ?? Tiebreakers.Default));

        return new SummaryViewModel(
            summary.LeagueName,
            division.DivisionName,
            division.Players.Select(GetPlayer).ToArray(),
            league?.MainDivisions ?? Array.Empty<IdName>());
    }

    private static PlayerScoreViewModel GetPlayer(SummaryPlayerScore playerScore)
    {
        var scores = new Dictionary<ScoreType, ScoreViewModel>
        {
            [ScoreType.Best] = GetScore(playerScore.Best),
            [ScoreType.Average] = GetScoreAverage(playerScore.Total, playerScore.Seasons),
            [ScoreType.Total] = GetScore(playerScore.Total)
        };

        return new PlayerScoreViewModel(
            playerScore.Player,
            scores,
            playerScore.Seasons
        );
    }

    private static ScoreViewModel GetScore(SummaryPlayerScoreDetails score) => GetScoreAverage(score);

    private static ScoreViewModel GetScoreAverage(SummaryPlayerScoreDetails score, int seasons = 1) => new(
        score.TotalPoints / seasons,
        (double)score.Wins / seasons,
        (double)score.Cla / seasons,
        (double)score.Supplies / seasons,
        (double)score.PowerTokens / seasons,
        score.MinutesPerMove,
        (double)score.Moves / seasons,
        GetHousesAverage(score.Houses, seasons),
        score.PenaltiesPoints / seasons,
        (double?)score.Position / seasons,
        score.Stats == null ? new Stats() : score.Stats / seasons);

    private static Dictionary<House, double> GetHousesAverage(IEnumerable<HousePoints> houses, int seasons) =>
        houses.ToDictionary(score => score.House, score => score.Points / seasons);
}