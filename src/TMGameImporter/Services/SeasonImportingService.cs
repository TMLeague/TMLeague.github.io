using Microsoft.Extensions.Logging;
using TMGameImporter.Files;
using TMModels;

namespace TMGameImporter.Services;

internal class SeasonImportingService
{
    private readonly DivisionImportingService _divisionImportingService;
    private readonly FileLoader _fileLoader;
    private readonly ILogger<SeasonImportingService> _logger;

    public SeasonImportingService(DivisionImportingService divisionImportingService, FileLoader fileLoader, ILogger<SeasonImportingService> logger)
    {
        _divisionImportingService = divisionImportingService;
        _fileLoader = fileLoader;
        _logger = logger;
    }

    public async Task Import(string leagueId, string seasonId, Scoring scoring, CancellationToken cancellationToken)
    {
        _logger.LogInformation("  Season {leagueId}/{seasonId} import started...",
            leagueId.ToUpper(), seasonId.ToUpper());

        var season = await _fileLoader.LoadSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
        {
            _logger.LogError("  Season {leagueId}/{seasonId} cannot be deserialized correctly.", 
                leagueId.ToUpper(), seasonId.ToUpper());
            return;
        }
        foreach (var divisionId in season.Divisions)
            await _divisionImportingService.Import(leagueId, seasonId, divisionId, scoring, cancellationToken);

        _logger.LogInformation("  Season {leagueId}/{seasonId} imported.",
            leagueId.ToUpper(), seasonId.ToUpper());
    }
}