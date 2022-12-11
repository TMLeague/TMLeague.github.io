using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Http.Json;
using TMLeague.Models;
using TMLeague.Models.ThroneMaster;

namespace TMLeague.Http
{
    public class LocalApi
    {
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly Logger<LocalApi> _logger;

        public LocalApi(IMemoryCache cache, HttpClient httpClient, Logger<LocalApi> logger)
        {
            _cache = cache;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Home?> GetHome(CancellationToken cancellationToken) =>
            await Get<Home>("Home",
                "/data/home.json", cancellationToken);

        public async Task<League?> GetLeague(string leagueId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(leagueId);

            return await Get<League>($"League \"{leagueId}\"",
                $"/data/leagues/{leagueId}/{leagueId}.json", cancellationToken);
        }

        public async Task<Season?> GetSeason(string? leagueId, string seasonId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(leagueId);
            ArgumentNullException.ThrowIfNull(seasonId);

            return await Get<Season>($"Season \"{leagueId}/{seasonId}\"",
                $"/data/leagues/{leagueId}/seasons/{seasonId}/{seasonId}.json", cancellationToken);
        }

        public async Task<Division?> GetDivision(string? leagueId, string seasonId, string divisionId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(leagueId);
            ArgumentNullException.ThrowIfNull(seasonId);
            ArgumentNullException.ThrowIfNull(divisionId);

            return await Get<Division>($"Division \"{leagueId}/{seasonId}/{divisionId}\"",
                $"/data/leagues/{leagueId}/seasons/{seasonId}/divisions/{divisionId}.json", cancellationToken);
        }

        public async Task<State?> GetState(uint gameId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(gameId);

            return await Get<State>($"State \"{gameId}\"", $"/data/games/{gameId}-state.json", cancellationToken);
        }

        public async Task<Log?> GetLog(uint gameId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(gameId);

            return await Get<Log>($"Log \"{gameId}\"", $"/data/games/{gameId}-log.json", cancellationToken);
        }

        public async Task<Game?> GetGame(uint gameId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(gameId);

            return await Get<Game>($"Game \"{gameId}\"", $"/data/games/{gameId}.json", cancellationToken);
        }

        private async Task<T?> Get<T>(string logName, string requestUri, CancellationToken cancellationToken) where T : class
        {
            if (_cache.TryGetValue<T>(requestUri, out var result))
                if (result != null)
                    return result;
            try
            {
                result = await _httpClient.GetFromJsonAsync<T>(requestUri, cancellationToken);
                _cache.Set(requestUri, result, TimeSpan.FromMinutes(1));
                return result;
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    _logger.LogWarning($"{logName} is not configured properly! It's configuration file should be here: \"{requestUri}\"");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"An error occurred while loading {logName}.");
                return null;
            }
        }
    }
}
