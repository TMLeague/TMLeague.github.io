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
            division.Houses.Select(GetHouse).ToArray(),
            league?.MainDivisions.Select(d => new IdName(d.Id, d.Name)).ToArray() ?? Array.Empty<IdName>());
    }

    private static PlayerScoreViewModel GetPlayer(SummaryPlayerScore playerScore)
    {
        var scores = new Dictionary<ScoreType, PlayerScoreDetailsViewModel>
        {
            [ScoreType.Best] = GetPlayerScore(playerScore.Best),
            [ScoreType.Average] = GetPlayerScoreAverage(playerScore.Total, playerScore.Seasons),
            [ScoreType.Total] = GetPlayerScore(playerScore.Total)
        };

        return new PlayerScoreViewModel(
            playerScore.Player,
            scores,
            playerScore.Seasons
        );
    }

    private static HouseScoreViewModel GetHouse(SummaryHouseScore houseScore)
    {
        var scores = new Dictionary<ScoreType, HouseScoreDetailsViewModel>
        {
            [ScoreType.Best] = GetHouseScore(houseScore.Best),
            [ScoreType.Average] = GetHouseScoreAverage(houseScore.Total, houseScore.Games),
            [ScoreType.Total] = GetHouseScore(houseScore.Total)
        };

        return new HouseScoreViewModel(
            houseScore.House,
            scores,
            houseScore.Games
        );
    }

    private static PlayerScoreDetailsViewModel GetPlayerScore(SummaryPlayerScoreDetails score) => GetPlayerScoreAverage(score);

    private static PlayerScoreDetailsViewModel GetPlayerScoreAverage(SummaryPlayerScoreDetails score, int seasons = 1) => new(
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
        score.Stats == null ? new Stats(score.Houses.Select(points => points.House)) : score.Stats / seasons);

    private static Dictionary<House, double> GetHousesAverage(IEnumerable<HousePoints> houses, int seasons) =>
        houses.ToDictionary(score => score.House, score => score.Points / seasons);

    private static HouseScoreDetailsViewModel GetHouseScore(SummaryHouseScoreDetails score) => GetHouseScoreAverage(score);

    private static HouseScoreDetailsViewModel GetHouseScoreAverage(SummaryHouseScoreDetails score, int games = 1) => new(
        score.Points / games,
        (double)score.Wins / games,
        (double)score.Cla / games,
        (double)score.Supplies / games,
        (double)score.PowerTokens / games,
        (double)score.Moves / games,
        score.Stats == null ? new Stats() : score.Stats / games);

    public async Task<SummaryInteractionsViewModel?> GetInteractions(string leagueId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);

        var interactions = await _dataProvider.GetLeagueInteractions(leagueId, cancellationToken);
        if (interactions == null)
            return null;

        return new SummaryInteractionsViewModel(league?.Name)
        {
            [ScoreType.Average] = interactions.Average(),
            [ScoreType.Total] = interactions
        };
    }
}