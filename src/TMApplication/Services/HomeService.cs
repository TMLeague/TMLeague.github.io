using TMApplication.Providers;
using TMApplication.ViewModels;

namespace TMApplication.Services;

public class HomeService
{
    private readonly IDataProvider _dataProvider;

    public HomeService(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<HomeViewModel> GetHomeVm(CancellationToken cancellationToken = default)
    {
        var home = await _dataProvider.GetHome(cancellationToken);
        if (home?.Leagues == null)
            return new HomeViewModel(Array.Empty<HomeLeagueButtonViewModel>());

        var leagues = new List<HomeLeagueButtonViewModel>();
        foreach (var leagueId in home.Leagues)
        {
            var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
            if (league is null)
                continue;

            leagues.Add(new HomeLeagueButtonViewModel(leagueId, league.Name, league.BackgroundImage));
        }

        return new HomeViewModel(leagues);
    }
}