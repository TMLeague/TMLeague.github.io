using System.Net;
using System.Net.Http.Json;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMInfrastructure.Http.Configuration;
using TMInfrastructure.Http.Converters;
using TMModels.ThroneMaster;

namespace TMInfrastructure.Http;

public class ThroneMasterApi
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly LogConverter _logConverter;
    private readonly IOptions<ThroneMasterApiOptions> _options;
    private readonly ILogger<ThroneMasterApi> _logger;

    public ThroneMasterApi(IMemoryCache cache,
        HttpClient httpClient, LogConverter logConverter,
        IOptions<ThroneMasterApiOptions> options, ILogger<ThroneMasterApi> logger)
    {
        _cache = cache;
        _httpClient = httpClient;
        _logConverter = logConverter;
        _options = options;
        _logger = logger;
    }

    public async Task<State?> GetState(uint gameId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameId);

        return await Get<State>($"TM state \"{gameId}\"",
            $"https://robwu.nl/https://game.thronemaster.net/ajax.php?game={gameId}&get=GAMEDATA",
            null, cancellationToken);
    }

    public async Task<Log?> GetLog(uint gameId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gameId);

        return await Get($"TM log \"{gameId}\"",
            $"https://robwu.nl/https://game.thronemaster.net/?game={gameId}&show=log",
            html => _logConverter.Convert(gameId, html),
            cancellationToken);
    }

    private async Task<T?> Get<T>(string logName, string requestUri,
        Func<HtmlDocument, T?>? convertTmResponse, CancellationToken cancellationToken) where T : class
    {
        if (_cache.TryGetValue(requestUri, out var cacheResult))
        {
            if (cacheResult is T result)
                return result;
            return null;
        }

        try
        {
            T? result;
            if (convertTmResponse != null)
            {
                var stringResult = await _httpClient.GetStringAsync(requestUri, cancellationToken);
                var html = new HtmlDocument();
                html.LoadHtml(stringResult);
                result = convertTmResponse(html);
            }
            else
                result = await _httpClient.GetFromJsonAsync<T>(requestUri, cancellationToken);

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