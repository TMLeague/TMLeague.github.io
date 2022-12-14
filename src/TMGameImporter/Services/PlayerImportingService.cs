using Microsoft.Extensions.Logging;
using TMGameImporter.Files;
using TMGameImporter.Http;
using TMGameImporter.Http.Converters;

namespace TMGameImporter.Services;

internal class PlayerImportingService
{
    private readonly ThroneMasterApi _api;
    private readonly PlayerConverter _converter;
    private readonly FileSaver _fileSaver;
    private readonly ILogger<PlayerImportingService> _logger;

    public PlayerImportingService(ThroneMasterApi api, PlayerConverter converter, FileSaver fileSaver, ILogger<PlayerImportingService> logger)
    {
        _api = api;
        _converter = converter;
        _fileSaver = fileSaver;
        _logger = logger;
    }

    public async Task Import(string playerName, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("    Player {playerName} import started...", playerName);

            var playerData = await _api.GetPlayer(playerName, cancellationToken);
            if (playerData == null)
            {
                _logger.LogError("    Player {playerName} cannot be fetched correctly.", playerName);
                return;
            }

            var (player, avatarUri) = await _converter.Convert(playerName, playerData);
            if (player == null)
            {
                _logger.LogError("    Player {playerName} cannot be converted correctly.", playerName);
                return;
            }

            if (avatarUri != null)
            {
                await using var avatarStream = await _api.GetImage(avatarUri, cancellationToken);
                if (avatarStream != null)
                    await _fileSaver.SavePlayerAvatar(avatarStream, playerName, cancellationToken);
            }

            await _fileSaver.SavePlayer(player, playerName, cancellationToken);
            _logger.LogInformation("    Player {playerName} imported.", playerName);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "    Player {playerName} import had unexpected errors.", playerName);
        }
    }
}