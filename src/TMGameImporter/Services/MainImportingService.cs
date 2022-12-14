using Microsoft.Extensions.Logging;
using TMGameImporter.Files;

namespace TMGameImporter.Services;

internal class MainImportingService
{
    private readonly LeagueImportingService _leagueImportingService;
    private readonly FileLoader _fileLoader;
    private readonly ILogger<MainImportingService> _logger;

    public MainImportingService(LeagueImportingService leagueImportingService, FileLoader fileLoader, ILogger<MainImportingService> logger)
    {
        _leagueImportingService = leagueImportingService;
        _fileLoader = fileLoader;
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
        foreach (var leagueId in home.Leagues)
            await _leagueImportingService.Import(leagueId, cancellationToken);

        _logger.LogInformation("Import finished.");
    }
}