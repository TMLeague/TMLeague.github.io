using System.Net;
using System.Net.Http.Json;
using TMLeague.Models;
using TMLeague.ViewModels;

namespace TMLeague.Services
{
    public class LeagueService
    {
        private readonly SeasonService _seasonService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LeagueService> _logger;

        public LeagueService(SeasonService seasonService, HttpClient httpClient, ILogger<LeagueService> logger)
        {
            _seasonService = seasonService;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<LeagueViewModel> GetLeagueVm(string id, CancellationToken cancellationToken)
        {
            var league = await GetLeague(id, cancellationToken);
            if (league == null)
                return new LeagueViewModel(id);

            if(league.Seasons == null)
                return new LeagueViewModel(id, league.Name);

            var seasons = new List<LeagueSeasonButtonViewModel>();
            foreach (var seasonId in league.Seasons)
            {
                var season = await _seasonService.GetSeason(id, seasonId, cancellationToken);
                if (season == null)
                    continue;

                seasons.Add(new LeagueSeasonButtonViewModel(seasonId, season.Name));
            }

            return new LeagueViewModel(id, league.Name, seasons);
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
                    _logger.LogWarning($"League \"{id}\" is not configured properly! It's configuration file should be here: \"/league/{id}/{id}.json\"");
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
