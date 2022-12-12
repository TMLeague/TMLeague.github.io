using TMLeague.Http;
using TMLeague.ViewModels;

namespace TMLeague.Services;

public class LeagueService
{
    private readonly LocalApi _localApi;

    public LeagueService(LocalApi localApi)
    {
        _localApi = localApi;
    }

    public async Task<LeagueViewModel> GetLeagueVm(string leagueId, CancellationToken cancellationToken)
    {
        var league = await _localApi.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return new LeagueViewModel(leagueId);

        if (league.Seasons == null)
            return new LeagueViewModel(leagueId, league.Name, league.Description, league.Rules, league.Discord);

        var seasons = new List<LeagueSeasonButtonViewModel>();
        foreach (var seasonId in league.Seasons)
        {
            var season = await _localApi.GetSeason(leagueId, seasonId, cancellationToken);
            if (season == null)
                continue;

            seasons.Add(new LeagueSeasonButtonViewModel(seasonId, season.Name));
        }

        return new LeagueViewModel(leagueId, league.Name, league.Description, league.Rules, league.Discord, seasons);
    }
}