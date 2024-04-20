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

    public async Task<(Summary?, TotalInteractions?)> Calculate(string leagueId, string leagueName, string seasonId,
        string[] leagueMainDivisions, CancellationToken cancellationToken)
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
            return (null, null);
        }

        var divisions = new List<SummaryDivision>();
        var interactions = new TotalInteractions();
        foreach (var divisionId in season.Divisions
                     .Where(leagueMainDivisions.Contains))
        {
            var summaryDivision = await _divisionSummaryCalculatingService.Calculate(
                leagueId, seasonId, divisionId, cancellationToken);
            if (summaryDivision != null)
                divisions.Add(summaryDivision);

            var divisionInteractions = await _fileLoader.LoadDivisionInteractions(leagueId, seasonId, divisionId, cancellationToken);
            if (divisionInteractions != null)
                interactions += divisionInteractions;
        }

        if (divisions.Count == 0)
        {
            _logger.LogWarning(
                "  Season {leagueId}/{seasonId} has no finished divisions.",
                leagueId.ToUpper(), seasonId.ToUpper());
            return (null, null);
        }

        _logger.LogInformation(
            "  Season {leagueId}/{seasonId} summary calculated.",
            leagueId.ToUpper(), seasonId.ToUpper());

        return (new Summary(leagueId, leagueName, divisions.ToArray()), interactions);
    }
}