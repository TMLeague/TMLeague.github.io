using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TMModels.ThroneMaster;

namespace TMGameImporter.Http;

internal class ThroneMasterApi : IThroneMasterDataProvider
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

    public async Task<StateRaw?> GetGameData(int gameId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameId);

        var dataString = await Get($"TM state \"{gameId}\"",
            $"ajax.php?game={gameId}&get=GAMEDATA",
            cancellationToken);

        if (dataString == null)
            return null;

        return JsonSerializer.Deserialize<StateRaw>(dataString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<StateRaw?> GetChat(int gameId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameId);

        var dataString = await Get($"TM chat \"{gameId}\"",
            $"ajax.php?game={gameId}&get=CHAT",
            cancellationToken);

        if (dataString == null)
            return null;

        return JsonSerializer.Deserialize<StateRaw>(dataString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<string?> GetLog(int gameId, CancellationToken cancellationToken)
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

    public async Task<Stream?> GetImage(string requestUri, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requestUri);

        if (_cache.TryGetValue(requestUri, out _))
            return null;

        try
        {
            var result = await _httpClient.GetStreamAsync(requestUri, cancellationToken);
            _cache.Set(requestUri, 0);

            return result;
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
                _logger.LogWarning($"Resource not found for path: \"{requestUri}\"");
            _cache.Set(requestUri, 0);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"An error occurred while loading resource from path \"{requestUri}\".");
            return null;
        }
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
            _logger.LogWarning(
                $"{logName} is not reachable ({ex.StatusCode}) under address: \"{requestUri}\"");

            if (ex.StatusCode == HttpStatusCode.NotFound)
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

internal interface IThroneMasterDataProvider
{
    public Task<StateRaw?> GetGameData(int gameId, CancellationToken cancellationToken);
    public Task<StateRaw?> GetChat(int gameId, CancellationToken cancellationToken);
    public Task<string?> GetLog(int gameId, CancellationToken cancellationToken);
    public Task<string?> GetPlayer(string playerName, CancellationToken cancellationToken);
    public Task<Stream?> GetImage(string requestUri, CancellationToken cancellationToken);
}