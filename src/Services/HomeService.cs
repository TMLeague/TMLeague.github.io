using TMLeague.Http;
using TMLeague.ViewModels;

namespace TMLeague.Services;

public class HomeService
{
    private readonly LocalApi _localApi;

    public HomeService(LocalApi localApi)
    {
        _localApi = localApi;
    }

    public async Task<HomeViewModel> GetHomeVm(CancellationToken cancellationToken = default)
    {
        var home = await _localApi.GetHome(cancellationToken);
        if (home?.Leagues == null)
            return new HomeViewModel(Array.Empty<HomeLeagueButtonViewModel>());

        var leagues = new List<HomeLeagueButtonViewModel>();
        foreach (var leagueId in home.Leagues)
        {
            var league = await _localApi.GetLeague(leagueId, cancellationToken);
            if (league is null)
                continue;

            leagues.Add(new HomeLeagueButtonViewModel(leagueId, league.Name, league.BackgroundImage));
        }

        return new HomeViewModel(leagues);
    }
}