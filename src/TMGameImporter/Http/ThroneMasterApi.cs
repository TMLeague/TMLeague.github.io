using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TMModels.ThroneMaster;

namespace TMGameImporter.Http;

internal class ThroneMasterApi(IMemoryCache cache, HttpClient client, ILogger<ThroneMasterApi> logger) : IThroneMasterDataProvider
{
    public async Task<StateRaw?> GetGameData(int gameId, CancellationToken cancellationToken)
    {
        var dataString = await Get($"TM state \"{gameId}\"",
            $"ajax.php?game={gameId}&get=GAMEDATA",
            cancellationToken);

        return dataString == null
            ? null
            : JsonSerializer.Deserialize<StateRaw>(dataString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<StateRaw?> GetChat(int gameId, CancellationToken cancellationToken)
    {
        var dataString = await Get($"TM chat \"{gameId}\"",
            $"ajax.php?game={gameId}&get=CHAT",
            cancellationToken);

        return dataString == null
            ? null
            : JsonSerializer.Deserialize<StateRaw>(dataString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<string?> GetLog(int gameId, CancellationToken cancellationToken)
    {
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
        if (cache.TryGetValue(requestUri, out var cacheResult))
            return cacheResult is string result ? result : null;

        try
        {
            var result = await client.GetStringAsync(requestUri, cancellationToken);
            cache.Set(requestUri, result);

            return result;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(
                $"{logName} is not reachable ({ex.StatusCode}) under address: \"{requestUri}\"");

            if (ex.StatusCode == HttpStatusCode.NotFound)
                cache.Set(requestUri, 0);

            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, $"An error occurred while loading {logName}.");
            return null;
        }
    }
}

internal interface IThroneMasterDataProvider
{
    public Task<StateRaw?> GetGameData(int gameId, CancellationToken cancellationToken);
    public Task<StateRaw?> GetChat(int gameId, CancellationToken cancellationToken);
    public Task<string?> GetLog(int gameId, CancellationToken cancellationToken);
}