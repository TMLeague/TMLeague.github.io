using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMGameImporter.Configuration;
using TMGameImporter.Files;
using TMModels;

namespace TMGameImporter.Services.Import;

internal class SeasonImportingService
{
    private readonly DivisionImportingService _divisionImportingService;
    private readonly FileLoader _fileLoader;
    private readonly IOptions<ImporterOptions> _options;
    private readonly ILogger<SeasonImportingService> _logger;

    public SeasonImportingService(DivisionImportingService divisionImportingService,
        FileLoader fileLoader, IOptions<ImporterOptions> options, ILogger<SeasonImportingService> logger)
    {
        _divisionImportingService = divisionImportingService;
        _fileLoader = fileLoader;
        _options = options;
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

        if (string.IsNullOrEmpty(_options.Value.Division))
        {
            foreach (var divisionId in season.Divisions)
                await _divisionImportingService.Import(leagueId, seasonId, divisionId, scoring, cancellationToken);
        }
        else
        {
            await _divisionImportingService.Import(leagueId, seasonId, _options.Value.Division, scoring, cancellationToken);
        }

        _logger.LogInformation("  Season {leagueId}/{seasonId} imported.",
            leagueId.ToUpper(), seasonId.ToUpper());
    }
}