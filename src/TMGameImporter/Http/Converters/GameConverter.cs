using Microsoft.Extensions.Logging;
using System.Text.Json;
using TMGameImporter.Http.Converters.Models;
using TMModels;
using TMModels.ThroneMaster;

namespace TMGameImporter.Http.Converters;

internal class GameConverter
{
    private readonly LogConverter _logConverter;
    private readonly StateConverter _stateConverter;
    private readonly ILogger<GameConverter> _logger;

    public GameConverter(LogConverter logConverter, StateConverter stateConverter, ILogger<GameConverter> logger)
    {
        _logConverter = logConverter;
        _stateConverter = stateConverter;
        _logger = logger;
    }

    public Game? Convert(uint gameId, string dataString, string chatString, string logHtmlString)
    {
        var stateRaw = JsonSerializer.Deserialize<StateRaw>(dataString);
        var chatRaw = JsonSerializer.Deserialize<StateRaw>(chatString);
        if (stateRaw == null)
            return null;

        var state = StateConverter.Convert(stateRaw, chatRaw);
        if (state.GameId != gameId)
        {
            _logger.LogError("Fetched game ID {fetchedGameId} is different than expected: {expectedGameId}.", state.GameId, gameId);
            return null;
        }
        var log = _logConverter.Convert(gameId, logHtmlString);

        bool isStalling = state.Chat?.Contains("delay!", StringComparison.InvariantCultureIgnoreCase) ?? false;

        var map = GetMap(state);
        var houses = GetHouses(state);
        return new Game(gameId, state.IsFinished, isStalling, state.Turn, map, houses);
    }

    private Map GetMap(State state)
    {
        throw new NotImplementedException();
    }

    private HouseData[] GetHouses(State state)
    {
        throw new NotImplementedException();
    }
}