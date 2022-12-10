using System.Net.Http.Json;
using TMLeague.Models;
using TMLeague.ViewModels;

namespace TMLeague.Services
{
    public class HomeService
    {
        private readonly LeagueService _leagueService;
        private readonly HttpClient _httpClient;

        public HomeService(LeagueService leagueService, HttpClient httpClient, ILogger<HomeService> logger)
        {
            _leagueService = leagueService;
            _httpClient = httpClient;
        }

        public async Task<HomeViewModel> GetHomeVm(CancellationToken cancellationToken)
        {
            var home = await _httpClient.GetFromJsonAsync<Home>("/data/home.json", cancellationToken);
            if (home?.League == null)
                return new HomeViewModel(Array.Empty<HomeLeagueButtonViewModel>());

            var leagues = new List<HomeLeagueButtonViewModel>();
            foreach (var leagueId in home.League)
            {
                var league = await _leagueService.GetLeague(leagueId, cancellationToken);
                if (league is null)
                    continue;

                leagues.Add(new HomeLeagueButtonViewModel(leagueId, league.Name, league.BackgroundImage));
            }

            return new HomeViewModel(leagues);
        }
    }
}
