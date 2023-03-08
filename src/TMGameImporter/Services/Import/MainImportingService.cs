using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMGameImporter.Configuration;
using TMGameImporter.Files;

namespace TMGameImporter.Services.Import;

internal class MainImportingService
{
    private readonly LeagueImportingService _leagueImportingService;
    private readonly FileLoader _fileLoader;
    private readonly IOptions<ImporterOptions> _options;
    private readonly ILogger<MainImportingService> _logger;

    public MainImportingService(LeagueImportingService leagueImportingService, 
        FileLoader fileLoader, IOptions<ImporterOptions> options, ILogger<MainImportingService> logger)
    {
        _leagueImportingService = leagueImportingService;
        _fileLoader = fileLoader;
        _options = options;
        _logger = logger;
    }

    public async Task Import(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Import started...");

        var home = await _fileLoader.LoadHome(cancellationToken);
        if (home == null)
        {
            _logger.LogError("Home cannot be deserialized correctly.");
            return;
        }

        if (string.IsNullOrEmpty(_options.Value.League))
        {
            foreach (var leagueId in home.Leagues)
                await _leagueImportingService.Import(leagueId, cancellationToken);
        }
        else
        {
            await _leagueImportingService.Import(_options.Value.League, cancellationToken);
        }

        _logger.LogInformation("Import finished.");
    }
}