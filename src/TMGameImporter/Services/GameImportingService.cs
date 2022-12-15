using Microsoft.Extensions.Logging;
using TMGameImporter.Files;
using TMGameImporter.Http;
using TMGameImporter.Http.Converters;

namespace TMGameImporter.Services;

internal class GameImportingService
{
    private readonly IThroneMasterDataProvider _api;
    private readonly GameConverter _converter;
    private readonly FileLoader _fileLoader;
    private readonly FileSaver _fileSaver;
    private readonly ILogger<PlayerImportingService> _logger;

    public GameImportingService(IThroneMasterDataProvider api, GameConverter converter, FileLoader fileLoader, FileSaver fileSaver, ILogger<PlayerImportingService> logger)
    {
        _api = api;
        _converter = converter;
        _fileLoader = fileLoader;
        _fileSaver = fileSaver;
        _logger = logger;
    }

    public async Task Import(uint gameId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("    Game {gameId} import started...", gameId);

            var game = await _fileLoader.LoadGame(gameId, cancellationToken);
            if (game?.IsFinished ?? false)
            {
                _logger.LogInformation("    Game {gameId} is already fetched and is finished.", gameId);
                return;
            }

            var gameData = await _api.GetGameData(gameId, CancellationToken.None);
            if (gameData == null)
            {
                _logger.LogError("    Game {gameId} data cannot be fetched correctly.", gameId);
                return;
            }

            var gameChat = await _api.GetChat(gameId, CancellationToken.None);
            if (gameChat == null)
            {
                _logger.LogError("    Game {gameId} chat cannot be fetched correctly.", gameId);
                return;
            }

            var gameLog = await _api.GetLog(gameId, CancellationToken.None);
            if (gameLog == null)
            {
                _logger.LogError("    Game {gameId} log cannot be fetched correctly.", gameId);
                return;
            }

            game = _converter.Convert(gameId, gameData, gameChat, gameLog);
            if (game == null)
            {
                _logger.LogError("    Game {gameId} cannot be converted correctly.", gameId);
                return;
            }

            await _fileSaver.SaveGame(game, gameId, cancellationToken);

            _logger.LogInformation("    Game {gameId} imported.", gameId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "    Game {gameId} import had unexpected errors.", gameId);
        }
    }
}