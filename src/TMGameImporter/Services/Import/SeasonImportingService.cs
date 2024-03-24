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

    public async Task Import(string leagueId, string seasonId, Scoring scoring, LeagueDivision[] leagueDivisions, CancellationToken cancellationToken)
    {
        _logger.LogInformation("  Season {leagueId}/{seasonId} import started...",
            leagueId.ToUpper(), seasonId.ToUpper());

        var season = await GetSeason(leagueId, seasonId, cancellationToken);
        if (season == null && seasonId != seasonId.ToLower())
        {
            seasonId = seasonId.ToLower();
            season = await GetSeason(leagueId, seasonId, cancellationToken);
        }
        if (season == null)
        {
            seasonId = $"s{seasonId.ToLower()}";
            season = await GetSeason(leagueId, seasonId, cancellationToken);
        }
        if (season == null)
            return;

        if (string.IsNullOrEmpty(_options.Value.Division))
        {
            foreach (var divisionId in season.Divisions)
            {
                var leagueDivision = leagueDivisions.FirstOrDefault(division => division.Id == divisionId);
                await _divisionImportingService.Import(leagueId, seasonId, divisionId, scoring,
                    leagueDivision?.Promotions, leagueDivision?.Relegations, cancellationToken);
            }
        }
        else
        {
            var leagueDivision = leagueDivisions.FirstOrDefault(division => division.Id == _options.Value.Division);
            await _divisionImportingService.Import(leagueId, seasonId, _options.Value.Division, scoring,
                leagueDivision?.Promotions, leagueDivision?.Relegations, cancellationToken);
        }

        _logger.LogInformation("  Season {leagueId}/{seasonId} imported.",
            leagueId.ToUpper(), seasonId.ToUpper());
    }

    private async Task<Season?> GetSeason(string leagueId, string seasonId, CancellationToken cancellationToken)
    {
        var season = await _fileLoader.LoadSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
            _logger.LogError("  Season {leagueId}/{seasonId} cannot be deserialized correctly.",
                leagueId.ToUpper(), seasonId.ToUpper());
        else
            _logger.LogInformation("  Season {leagueId}/{seasonId} deserialized correctly.",
                leagueId.ToUpper(), seasonId.ToUpper());
        return season;
    }
}