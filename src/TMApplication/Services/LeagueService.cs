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

    public async Task<SeasonChampionViewModel?> GetLeagueChampionVm(string leagueId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return null;

        if (league.Seasons.Length < 1)
            return null;

        var champion = await _seasonService.GetSeasonChampionVm(leagueId, league.Seasons.Last(), cancellationToken);
        if (!string.IsNullOrEmpty(champion.PlayerName))
            return champion;

        if (league.Seasons.Length < 2)
            return null;

        return await _seasonService.GetSeasonChampionVm(leagueId, league.Seasons[^2], cancellationToken);
    }

    public async Task<DivisionSetupViewModel?> GetDivisionSetupVm(string leagueId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return null;

        var nextMainSeason = league.Seasons
            .Max(season =>
                int.TryParse(season[1..], out var seasonNumber) ?
                    seasonNumber + 1 :
                    1)
            .ToString();

        return new DivisionSetupViewModel(
            league.Name,
            league.InitialMessage?.SpecialNote,
            nextMainSeason);
    }
}