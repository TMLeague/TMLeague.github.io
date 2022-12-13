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
}