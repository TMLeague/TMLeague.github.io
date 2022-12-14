using Microsoft.Extensions.Logging;
using TMGameImporter.Files;

namespace TMGameImporter.Services;

internal class LeagueImportingService
{
    private readonly SeasonImportingService _seasonImportingService;
    private readonly FileLoader _fileLoader;
    private readonly ILogger<LeagueImportingService> _logger;

    public LeagueImportingService(SeasonImportingService seasonImportingService, FileLoader fileLoader, ILogger<LeagueImportingService> logger)
    {
        _seasonImportingService = seasonImportingService;
        _fileLoader = fileLoader;
        _logger = logger;
    }

    public async Task Import(string leagueId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(" League {leagueId} import started...", leagueId.ToUpper());

        var league = await _fileLoader.LoadLeague(leagueId, cancellationToken);
        if (league == null)
        {
            _logger.LogError(" League {leagueId} cannot be deserialized correctly.", 
                leagueId.ToUpper());
            return;
        }
        foreach (var seasonId in league.Seasons)
            await _seasonImportingService.Import(leagueId, seasonId, cancellationToken);

        _logger.LogInformation(" League {leagueId} imported.", leagueId.ToUpper());
    }
}