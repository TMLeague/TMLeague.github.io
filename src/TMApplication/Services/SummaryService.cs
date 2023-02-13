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

    public async Task<SummaryViewModel?> GetSummaryVm(string leagueId, CancellationToken cancellationToken = default)
    {
        var summary = await _dataProvider.GetSummary(leagueId, cancellationToken);
        if (summary == null)
            return null;

        return new SummaryViewModel(
            summary.LeagueName,
            summary.Divisions.Select(GetDivision).ToArray());
    }

    private static SummaryDivisionViewModel GetDivision(SummaryDivision division) => new(
        division.DivisionId,
        division.DivisionName,
        division.Players.Select(GetPlayer).ToArray()
    );

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

    private static ScoreViewModel GetScore(SummaryScore score) => GetScoreAverage(score, 1);

    private static ScoreViewModel GetScoreAverage(SummaryScore score, uint seasons) => new(
        (double)score.TotalPoints / seasons,
        (double)score.Wins / seasons,
        (double)score.Cla / seasons,
        (double)score.Supplies / seasons,
        (double)score.PowerTokens / seasons,
        score.MinutesPerMove,
        (double)score.Moves / seasons,
        GetHousesAverage(score.Houses, seasons),
        (double)score.PenaltiesPoints / seasons);

    private static Dictionary<House, double> GetHousesAverage(IEnumerable<SummaryHouseScore> houses, uint seasons) =>
        houses.ToDictionary(score => score.House, score => (double)score.Points / seasons);
}