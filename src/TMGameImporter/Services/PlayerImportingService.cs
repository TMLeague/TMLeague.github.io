using Microsoft.Extensions.Logging;
using TMGameImporter.Files;
using TMGameImporter.Http;

namespace TMGameImporter.Services;

internal class PlayerImportingService
{
    private readonly ThroneMasterApi _api;
    private readonly FileSaver _fileSaver;
    private readonly ILogger<PlayerImportingService> _logger;

    public PlayerImportingService(ThroneMasterApi api, FileSaver fileSaver, ILogger<PlayerImportingService> logger)
    {
        _api = api;
        _fileSaver = fileSaver;
        _logger = logger;
    }

    public async Task Import(string playerName, CancellationToken cancellationToken)
    {
        var playerData = await _api.GetPlayer(playerName, cancellationToken);
        if (playerData == null)
        {
            _logger.LogError("Player {playerName} cannot be fetched correctly.", playerName);
            return;
        }
        await _fileSaver.SavePlayer(playerData, playerName, cancellationToken);
    }
}