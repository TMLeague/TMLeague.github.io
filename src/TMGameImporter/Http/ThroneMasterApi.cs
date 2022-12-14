using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;

namespace TMGameImporter.Http;

internal class ThroneMasterApi
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ThroneMasterApi> _logger;

    public ThroneMasterApi(IMemoryCache cache, HttpClient httpClient, ILogger<ThroneMasterApi> logger)
    {
        _cache = cache;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GetGameData(uint gameId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameId);

        return await Get($"TM state \"{gameId}\"",
            $"ajax.php?game={gameId}&get=GAMEDATA",
            cancellationToken);
    }

    public async Task<string?> GetChat(uint gameId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameId);

        return await Get($"TM chat \"{gameId}\"",
            $"ajax.php?game={gameId}&get=CHAT",
            cancellationToken);
    }

    public async Task<string?> GetLog(uint gameId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameId);

        return await Get($"TM log \"{gameId}\"",
            $"?game={gameId}&show=log",
            cancellationToken);
    }

    public async Task<string?> GetPlayer(string playerName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(playerName);

        return await Get($"TM player \"{playerName}\"",
            $"https://www.thronemaster.net/?goto=community&sub=members&usr={playerName}",
            cancellationToken);
    }

    private async Task<string?> Get(string logName, string requestUri, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(requestUri, out var cacheResult))
        {
            if (cacheResult is string result)
                return result;
            return null;
        }

        try
        {
            var result = await _httpClient.GetStringAsync(requestUri, cancellationToken);
            _cache.Set(requestUri, result);

            return result;
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
                _logger.LogWarning($"{logName} is not configured properly! It's configuration file should be here: \"{requestUri}\"");
            _cache.Set(requestUri, 0);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"An error occurred while loading {logName}.");
            return null;
        }
    }
}