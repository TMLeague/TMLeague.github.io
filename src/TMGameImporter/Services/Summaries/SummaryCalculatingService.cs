using Microsoft.Extensions.Logging;
using TMGameImporter.Files;

namespace TMGameImporter.Services.Summaries;

internal class SummaryCalculatingService
{
    private readonly LeagueSummaryCalculatingService _leagueSummaryCalculatingService;
    private readonly FileLoader _fileLoader;
    private readonly FileSaver _fileSaver;
    private readonly ILogger<SummaryCalculatingService> _logger;

    public SummaryCalculatingService(LeagueSummaryCalculatingService leagueSummaryCalculatingService,
        FileLoader fileLoader, FileSaver fileSaver, ILogger<SummaryCalculatingService> logger)
    {
        _leagueSummaryCalculatingService = leagueSummaryCalculatingService;
        _fileLoader = fileLoader;
        _fileSaver = fileSaver;
        _logger = logger;
    }

    public async Task Calculate(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Summary calculation started...");

        var home = await _fileLoader.LoadHome(cancellationToken);
        if (home == null)
        {
            _logger.LogError("Home cannot be deserialized correctly.");
            return;
        }

        foreach (var leagueId in home.Leagues)
        {
            try
            {
                var leagueSummary = await _leagueSummaryCalculatingService.Calculate(leagueId, cancellationToken);
                if (leagueSummary != null)
                    await _fileSaver.SaveSummary(leagueSummary, leagueId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred during league {leagueId} summary calculation.",
                    leagueId.ToUpper());
            }
        }

        _logger.LogInformation("Summary calculation finished.");
    }
}