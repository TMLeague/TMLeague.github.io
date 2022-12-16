using TMApplication.Providers;
using TMApplication.ViewModels;

namespace TMApplication.Services;

public class SeasonService
{
    private readonly IDataProvider _dataProvider;

    public SeasonService(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<SeasonSummaryViewModel> GetSeasonSummaryVm(
        string leagueId, string seasonId, CancellationToken cancellationToken = default)
    {
        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        return new SeasonSummaryViewModel(leagueId, seasonId, season?.Name,
            season?.Divisions ?? Array.Empty<string>());
    }

    public async Task<SeasonChampionViewModel> GetSeasonChampionVm(
        string leagueId, string seasonId, CancellationToken cancellationToken)
    {
        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
            return new SeasonChampionViewModel();

        var divisionId = season.Divisions.First();
        var division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);
        if (division == null)
            return new SeasonChampionViewModel();

        if (division.IsFinished)
        {
            var results = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);
            if (results != null)
                return new SeasonChampionViewModel(results.Players.First().Player, division.WinnerTitle);
        }

        return new SeasonChampionViewModel();
    }
}