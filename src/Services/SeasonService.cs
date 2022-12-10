using System.Net;
using System.Net.Http.Json;
using TMLeague.Models;

namespace TMLeague.Services
{
    public class SeasonService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SeasonService> _logger;

        public SeasonService(HttpClient httpClient, ILogger<SeasonService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Season?> GetSeason(string? leagueId, string? seasonId, CancellationToken cancellationToken)
        {
            if (leagueId == null)
                return null;

            try
            {
                return await _httpClient.GetFromJsonAsync<Season>($"/data/leagues/{leagueId}/seasons/{seasonId}/{seasonId}.json", cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    _logger.LogWarning($"League \"{leagueId}\" is not configured properly! It's configuration file should be here: \"/league/{leagueId}/{leagueId}.json\"");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"An error occurred while loading league {leagueId}");
                return null;
            }
        }
    }
}
