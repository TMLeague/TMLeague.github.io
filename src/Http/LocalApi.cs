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
        private readonly ILogger<LocalApi> _logger;

        private readonly TimeSpan _defaultExpirationTime = TimeSpan.FromMinutes(3);
        private readonly TimeSpan _notFoundExpirationTime = TimeSpan.FromMinutes(5);

        public LocalApi(IMemoryCache cache, HttpClient httpClient, ILogger<LocalApi> logger)
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
            if (_cache.TryGetValue(requestUri, out var result))
            {
                if (result is T tResult)
                    return tResult;
                return null;
            }

            try
            {
                var tResult = await _httpClient.GetFromJsonAsync<T>(requestUri, cancellationToken);
                if (tResult != null)
                    _cache.Set(requestUri, tResult, _defaultExpirationTime);
                return tResult;
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                    _logger.LogWarning($"{logName} is not configured properly! It's configuration file should be here: \"{requestUri}\"");
                _cache.Set(requestUri, 0, _notFoundExpirationTime);
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
