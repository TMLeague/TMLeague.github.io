using TMLeague.Http;
using TMLeague.ViewModels;

namespace TMLeague.Services;

public class SeasonService
{
    private readonly LocalApi _localApi;

    public SeasonService(LocalApi localApi)
    {
        _localApi = localApi;
    }

    public async Task<SeasonSummaryViewModel> GetSeasonSummaryVm(
        string leagueId, string seasonId, CancellationToken cancellationToken = default)
    {
        var season = await _localApi.GetSeason(leagueId, seasonId, cancellationToken);
        return new SeasonSummaryViewModel(leagueId, seasonId, season?.Name,
            season?.Divisions ?? Array.Empty<string>());
    }
}