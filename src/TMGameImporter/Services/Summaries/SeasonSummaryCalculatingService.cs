using Microsoft.Extensions.Logging;
using TMGameImporter.Files;
using TMModels;

namespace TMGameImporter.Services.Summaries;

internal class SeasonSummaryCalculatingService
{
    private readonly DivisionSummaryCalculatingService _divisionSummaryCalculatingService;
    private readonly FileLoader _fileLoader;
    private readonly ILogger<LeagueSummaryCalculatingService> _logger;

    public SeasonSummaryCalculatingService(
        DivisionSummaryCalculatingService divisionSummaryCalculatingService,
        FileLoader fileLoader, ILogger<LeagueSummaryCalculatingService> logger)
    {
        _divisionSummaryCalculatingService = divisionSummaryCalculatingService;
        _fileLoader = fileLoader;
        _logger = logger;
    }

    public async Task<Summary?> Calculate(string leagueId, string seasonId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "  Season {leagueId}/{seasonId} summary calculation started...", 
            leagueId.ToUpper(), seasonId.ToUpper());

        var season = await _fileLoader.LoadSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
        {
            _logger.LogError(
                "  Season {leagueId}/{seasonId} cannot be deserialized correctly.",
                leagueId.ToUpper(), seasonId.ToUpper());
            return null;
        }

        var divisions = new List<SummaryDivision>();
        foreach (var divisionId in season.Divisions)
        {
            var summaryDivision = await _divisionSummaryCalculatingService.Calculate(leagueId, seasonId, divisionId, cancellationToken);
            if (summaryDivision != null)
                divisions.Add(summaryDivision);
        }

        _logger.LogInformation(
            "  Season {leagueId}/{seasonId} summary calculated.",
            leagueId.ToUpper(), seasonId.ToUpper());

        return new Summary(leagueId, divisions.ToArray());
    }
}