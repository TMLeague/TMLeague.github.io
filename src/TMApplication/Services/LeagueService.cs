using TMApplication.Providers;
using TMApplication.ViewModels;

namespace TMApplication.Services;

public class LeagueService
{
    private readonly IDataProvider _dataProvider;

    public LeagueService(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<LeagueViewModel> GetLeagueVm(string leagueId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return new LeagueViewModel(leagueId);

        if (league.Seasons == null)
            return new LeagueViewModel(leagueId, league.Name, league.Description, league.Rules, league.Discord);

        var seasons = new List<LeagueSeasonButtonViewModel>();
        foreach (var seasonId in league.Seasons)
        {
            var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
            if (season == null)
                continue;

            seasons.Add(new LeagueSeasonButtonViewModel(seasonId, season.Name));
        }

        return new LeagueViewModel(leagueId, league.Name, league.Description, league.Rules, league.Discord, seasons);
    }
}