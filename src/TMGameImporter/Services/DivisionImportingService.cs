using Microsoft.Extensions.Logging;
using TMGameImporter.Files;
using TMModels;

namespace TMGameImporter.Services;

internal class DivisionImportingService
{
    private readonly GameImportingService _gameImportingService;
    private readonly PlayerImportingService _playerImportingService;
    private readonly FileLoader _fileLoader;
    private readonly ILogger<DivisionImportingService> _logger;

    public DivisionImportingService(GameImportingService gameImportingService, PlayerImportingService playerImportingService, FileLoader fileLoader, ILogger<DivisionImportingService> logger)
    {
        _gameImportingService = gameImportingService;
        _playerImportingService = playerImportingService;
        _fileLoader = fileLoader;
        _logger = logger;
    }

    public async Task Import(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("   Division {leagueId}/{seasonId}/{divisionId} import started...",
            leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());

        var division = await _fileLoader.LoadDivision(leagueId, seasonId, divisionId, cancellationToken);
        if (division == null)
        {
            _logger.LogError("   Division {leagueId}/{seasonId}/{divisionId} cannot be deserialized correctly.",
                leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
            return;
        }
        foreach (var playerName in division.Players)
            await _playerImportingService.Import(playerName, cancellationToken);
        foreach (var gameId in division.Games)
            await _gameImportingService.Import(gameId, cancellationToken);

        _logger.LogInformation("   Division {leagueId}/{seasonId}/{divisionId} imported.", 
            leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
    }
}