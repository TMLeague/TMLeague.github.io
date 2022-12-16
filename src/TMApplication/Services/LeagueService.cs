using TMApplication.Providers;
using TMApplication.ViewModels;

namespace TMApplication.Services;

public class LeagueService
{
    private readonly IDataProvider _dataProvider;
    private readonly SeasonService _seasonService;

    public LeagueService(IDataProvider dataProvider, SeasonService seasonService)
    {
        _dataProvider = dataProvider;
        _seasonService = seasonService;
    }

    public async Task<LeagueViewModel> GetLeagueVm(string leagueId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return new LeagueViewModel(leagueId);

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

    public async Task<SeasonChampionViewModel> GetLeagueChampionVm(string leagueId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return new SeasonChampionViewModel();

        if (league.Seasons.Length < 1)
            return new SeasonChampionViewModel();

        var champion = await _seasonService.GetSeasonChampionVm(leagueId, league.Seasons.Last(), cancellationToken);
        if (!string.IsNullOrEmpty(champion.PlayerName))
            return champion;

        if (league.Seasons.Length < 2)
            return new SeasonChampionViewModel();

        return await _seasonService.GetSeasonChampionVm(leagueId, league.Seasons[^2], cancellationToken);
    }
}