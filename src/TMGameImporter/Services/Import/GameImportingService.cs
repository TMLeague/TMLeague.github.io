using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMGameImporter.Configuration;
using TMGameImporter.Files;
using TMGameImporter.Http;
using TMGameImporter.Http.Converters;
using TMModels;

namespace TMGameImporter.Services.Import;

internal class GameImportingService
{
    private readonly IThroneMasterDataProvider _api;
    private readonly GameConverter _converter;
    private readonly FileLoader _fileLoader;
    private readonly FileSaver _fileSaver;
    private readonly IOptions<ImporterOptions> _options;
    private readonly ILogger<GameImportingService> _logger;

    public GameImportingService(IThroneMasterDataProvider api, GameConverter converter, FileLoader fileLoader, FileSaver fileSaver, IOptions<ImporterOptions> options, ILogger<GameImportingService> logger)
    {
        _api = api;
        _converter = converter;
        _fileLoader = fileLoader;
        _fileSaver = fileSaver;
        _options = options;
        _logger = logger;
    }

    public async Task<Game?> Import(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("    Game {gameId} import started...", gameId);

            Game? game;
            try
            {
                game = await _fileLoader.LoadGame(gameId, cancellationToken);
                if (game?.IsCreatedManually ?? false)
                {
                    _logger.LogInformation("    Game {gameId} is created manually and can't be overriden.", gameId);
                    return game;
                }

                if ((game?.IsFinished ?? false) && !_options.Value.FetchFinishedGames)
                {
                    _logger.LogInformation("    Game {gameId} is already fetched and is finished.", gameId);
                    return game;
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "    Game {gameId} can't be loaded.", gameId);
            }

            var gameData = await _api.GetGameData(gameId, CancellationToken.None);
            if (gameData == null)
            {
                _logger.LogError("    Game {gameId} data cannot be fetched correctly.", gameId);
                return null;
            }

            var gameChat = await _api.GetChat(gameId, CancellationToken.None);
            if (gameChat == null)
            {
                _logger.LogError("    Game {gameId} chat cannot be fetched correctly.", gameId);
                return null;
            }

            var gameLog = await _api.GetLog(gameId, CancellationToken.None);
            if (gameLog == null)
            {
                _logger.LogError("    Game {gameId} log cannot be fetched correctly.", gameId);
                return null;
            }

            game = _converter.Convert(gameId, gameData, gameChat, gameLog);
            if (game == null)
            {
                _logger.LogError("    Game {gameId} cannot be converted correctly.", gameId);
                return null;
            }

            await _fileSaver.SaveGame(game, gameId, cancellationToken);

            _logger.LogInformation("    Game {gameId} imported.", gameId);

            return game;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "    Game {gameId} import had unexpected errors.", gameId);

            return null;
        }
    }
}