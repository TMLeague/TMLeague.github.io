using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TMApplication.Providers;
using TMInfrastructure.Http.Configuration;
using TMModels;

namespace TMInfrastructure.Http;

public class LocalApi : IDataProvider
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly IOptions<LocalApiOptions> _options;
    private readonly ILogger<LocalApi> _logger;

    public LocalApi(IMemoryCache cache, HttpClient httpClient, IOptions<LocalApiOptions> options, ILogger<LocalApi> logger)
    {
        _cache = cache;
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<Home?> GetHome(CancellationToken cancellationToken) =>
        await Get<Home>("Home",
            "/data/home.json", cancellationToken);

    public async Task<string[]?> GetPasswords(CancellationToken cancellationToken) =>
        await Get<string[]>("Passwords",
            "/data/passwords.json", cancellationToken);

    public async Task<League?> GetLeague(string leagueId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(leagueId);

        return await Get<League>($"League \"{leagueId.ToUpper()}\"",
            $"/data/leagues/{leagueId}/{leagueId}.json", cancellationToken);
    }

    public async Task<Season?> GetSeason(string? leagueId, string seasonId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(leagueId);
        ArgumentNullException.ThrowIfNull(seasonId);

        return await Get<Season>($"Season \"{leagueId.ToUpper()}/{seasonId.ToUpper()}\"",
            $"/data/leagues/{leagueId}/seasons/{seasonId}/{seasonId}.json", cancellationToken);
    }

    public async Task<Division?> GetDivision(string? leagueId, string seasonId, string divisionId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(leagueId);
        ArgumentNullException.ThrowIfNull(seasonId);
        ArgumentNullException.ThrowIfNull(divisionId);

        return await Get<Division>($"Division \"{leagueId.ToUpper()}/{seasonId.ToUpper()}/{divisionId.ToUpper()}\"",
            $"/data/leagues/{leagueId}/seasons/{seasonId}/divisions/{divisionId}.json", cancellationToken);
    }

    public async Task<Game?> GetGame(int gameId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameId);

        return await Get<Game>($"Game \"{gameId}\"",
            $"/data/games/{gameId}.json", cancellationToken);
    }

    public async Task<Results?> GetResults(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(leagueId);
        ArgumentNullException.ThrowIfNull(seasonId);
        ArgumentNullException.ThrowIfNull(divisionId);

        return await Get<Results>($"Results \"{leagueId.ToUpper()}/{seasonId.ToUpper()}/{divisionId.ToUpper()}\"",
            $"/data/results/{leagueId}/{seasonId}/{divisionId}.json", cancellationToken);
    }

    public async Task<Draft[]> GetDrafts(int players, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(players);

        return await Get<Draft[]>($"Drafts for {players}p", $"/data/drafts/p{players}.json", cancellationToken) ?? Array.Empty<Draft>();
    }

    public async Task<Summary?> GetSummary(string leagueId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(leagueId);

        return await Get<Summary>($"League \"{leagueId.ToUpper()}\"",
            $"/data/results/{leagueId}/summary.json", cancellationToken);
    }

    public async Task<Player?> GetPlayer(string playerName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(playerName);

        return await Get<Player>($"Player \"{playerName.ToUpper()}\"",
            $"/data/players/{playerName}.json", cancellationToken);
    }

    public async Task<TotalInteractions?> GetLeagueInteractions(string leagueId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(leagueId);

        return await Get<TotalInteractions>($"League interactions \"{leagueId.ToUpper()}\"",
            $"/data/results/{leagueId}/interactions.json", cancellationToken);
    }

    public async Task<TotalInteractions?> GetDivisionInteractions(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(leagueId);
        ArgumentNullException.ThrowIfNull(seasonId);
        ArgumentNullException.ThrowIfNull(divisionId);

        return await Get<TotalInteractions>($"Player interactions \"{leagueId.ToUpper()}/{seasonId.ToUpper()}/{divisionId.ToUpper()}\"",
            $"/data/results/{leagueId}/{seasonId}/{divisionId}.interactions.json", cancellationToken);
    }

    private async Task<T?> Get<T>(string logName, string requestUri, CancellationToken cancellationToken) where T : class
    {
        if (_cache.TryGetValue(requestUri, out var cacheResult))
        {
            if (cacheResult is T result)
                return result;
            return null;
        }

        try
        {
            var result = await _httpClient.GetFromJsonAsync<T>(requestUri, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
            if (result != null)
                _cache.Set(requestUri, result, _options.Value.Cache.DefaultExpirationTime);
            return result;
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
                _logger.LogWarning($"{logName} is not configured properly! It's configuration file should be here: \"{requestUri}\"");
            _cache.Set(requestUri, 0, _options.Value.Cache.NotFoundExpirationTime);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"An error occurred while loading {logName}.");
            return null;
        }
    }
}