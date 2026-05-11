using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMGameImporter.Configuration;
using TMGameImporter.Files;
using TMGameImporter.Http;
using TMGameImporter.Http.Converters;
using TMModels;

namespace TMGameImporter.Services.Import;

internal class GameImportingService(IThroneMasterDataProvider api, GameConverter converter, FileLoader fileLoader, FileSaver fileSaver, IOptions<ImporterOptions> options, ILogger<GameImportingService> logger)
{
    public async Task<Game?> Import(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("    Game {gameId} import started...", gameId);

            Game? game;
            try
            {
                game = await fileLoader.LoadGame(gameId, cancellationToken);
                if (game?.IsCreatedManually ?? false)
                {
                    logger.LogInformation("    Game {gameId} is created manually and can't be overriden.", gameId);
                    return game;
                }

                if ((game?.IsFinished ?? false) && !options.Value.FetchFinishedGames)
                {
                    logger.LogInformation("    Game {gameId} is already fetched and is finished.", gameId);
                    return game;
                }
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "    Game {gameId} can't be loaded.", gameId);
            }

            var gameData = await api.GetGameData(gameId, CancellationToken.None);
            if (gameData == null)
            {
                logger.LogError("    Game {gameId} data cannot be fetched correctly.", gameId);
                return null;
            }

            var gameChat = await api.GetChat(gameId, CancellationToken.None);
            if (gameChat == null)
            {
                logger.LogError("    Game {gameId} chat cannot be fetched correctly.", gameId);
                return null;
            }

            var gameLog = await api.GetLog(gameId, CancellationToken.None);
            if (gameLog == null)
            {
                logger.LogError("    Game {gameId} log cannot be fetched correctly.", gameId);
                return null;
            }

            game = converter.Convert(gameId, gameData, gameChat, gameLog);
            if (game == null)
            {
                logger.LogError("    Game {gameId} cannot be converted correctly.", gameId);
                return null;
            }

            await fileSaver.SaveGame(game, gameId, cancellationToken);

            logger.LogInformation("    Game {gameId} imported.", gameId);

            return game;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "    Game {gameId} import had unexpected errors.", gameId);

            return null;
        }
    }
}