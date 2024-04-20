using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMGameImporter.Configuration;
using TMGameImporter.Files;

namespace TMGameImporter.Services.Summaries;

internal class SummaryCalculatingService
{
    private readonly LeagueSummaryCalculatingService _leagueSummaryCalculatingService;
    private readonly FileLoader _fileLoader;
    private readonly FileSaver _fileSaver;
    private readonly IOptions<ImporterOptions> _options;
    private readonly ILogger<SummaryCalculatingService> _logger;

    public SummaryCalculatingService(LeagueSummaryCalculatingService leagueSummaryCalculatingService,
        FileLoader fileLoader, FileSaver fileSaver, IOptions<ImporterOptions> options, ILogger<SummaryCalculatingService> logger)
    {
        _leagueSummaryCalculatingService = leagueSummaryCalculatingService;
        _fileLoader = fileLoader;
        _fileSaver = fileSaver;
        _options = options;
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

        if (string.IsNullOrEmpty(_options.Value.League))
        {
            foreach (var leagueId in home.Leagues)
                await CalculateSummary(leagueId, cancellationToken);
        }
        else
        {
            await CalculateSummary(_options.Value.League, cancellationToken);
        }

        _logger.LogInformation("Summary calculation finished.");
    }

    private async Task CalculateSummary(string leagueId, CancellationToken cancellationToken)
    {
        try
        {
            var (leagueSummary, interactions) = await _leagueSummaryCalculatingService.Calculate(leagueId, cancellationToken);
            if (leagueSummary != null)
                await _fileSaver.SaveSummary(leagueSummary, leagueId, cancellationToken);
            if (interactions != null)
                await _fileSaver.SaveLeagueInteractions(interactions, leagueId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An error occurred during league {leagueId} summary calculation.",
                leagueId.ToUpper());
        }
    }
}