using Microsoft.Extensions.Logging;
using TMGameImporter.Files;
using TMGameImporter.Http;

namespace TMGameImporter.Services;

internal class GameImportingService
{
    private readonly ThroneMasterApi _api;
    private readonly FileSaver _fileSaver;
    private readonly ILogger<PlayerImportingService> _logger;

    public GameImportingService(ThroneMasterApi api, FileSaver fileSaver, ILogger<PlayerImportingService> logger)
    {
        _api = api;
        _fileSaver = fileSaver;
        _logger = logger;
    }

    public async Task Import(uint gameId, CancellationToken cancellationToken)
    {
        var gameData = await _api.GetGameData(gameId, CancellationToken.None);
        if (gameData == null)
        {
            _logger.LogError("Game {gameId} data cannot be fetched correctly.", gameId);
            return;
        }
        await _fileSaver.SaveGameData(gameData, gameId, cancellationToken);

        var gameChat = await _api.GetChat(gameId, CancellationToken.None);
        if (gameChat == null)
        {
            _logger.LogError("Game {gameId} chat cannot be fetched correctly.", gameId);
            return;
        }
        await _fileSaver.SaveGameChat(gameChat, gameId, cancellationToken);

        var gameLog = await _api.GetLog(gameId, CancellationToken.None);
        if (gameLog == null)
        {
            _logger.LogError("Game {gameId} log cannot be fetched correctly.", gameId);
            return;
        }
        await _fileSaver.SaveGameLog(gameLog, gameId, cancellationToken);
    }
}