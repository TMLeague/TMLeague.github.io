using System.Net;
using System.Net.Http.Json;
using TMLeague.Models;
using TMLeague.ViewModels;

namespace TMLeague.Services
{
    public class LeaguesService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LeaguesService> _logger;

        public LeaguesService(HttpClient httpClient, ILogger<LeaguesService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<LeaguesViewModel> GetLeagues(CancellationToken cancellationToken)
        {
            var leagueIds = await _httpClient.GetFromJsonAsync<string[]>("/data/leagues/leagues.json", cancellationToken);
            if (leagueIds == null)
                return new LeaguesViewModel(Array.Empty<LeagueViewModel>());

            var leagueVmList = new List<LeagueViewModel>();
            foreach (var leagueId in leagueIds)
            {
                var league = await GetLeague(leagueId, cancellationToken);
                if (league is null)
                    continue;

                leagueVmList.Add(new LeagueViewModel(leagueId, league.Name, null));
            }

            return new LeaguesViewModel(leagueVmList);
        }

        public async Task<League?> GetLeague(string? id, CancellationToken cancellationToken)
        {
            if (id == null)
                return null;

            try
            {
                return await _httpClient.GetFromJsonAsync<League>($"/data/leagues/{id}/{id}.json", cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound) 
                    _logger.LogWarning($"League \"{id}\" is not configured properly! It's configuration file should be here: \"/data/leagues/{id}/{id}.json\"");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"An error occurred while loading league {id}");
                return null;
            }
        }
    }
}
