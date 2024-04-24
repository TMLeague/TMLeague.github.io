using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMGameImporter.Configuration;
using TMGameImporter.Files;
using TMModels;

namespace TMGameImporter.Services.Import;

internal class LeagueImportingService
{
    private readonly SeasonImportingService _seasonImportingService;
    private readonly FileLoader _fileLoader;
    private readonly IOptions<ImporterOptions> _options;
    private readonly ILogger<LeagueImportingService> _logger;

    public LeagueImportingService(SeasonImportingService seasonImportingService,
        FileLoader fileLoader, IOptions<ImporterOptions> options, ILogger<LeagueImportingService> logger)
    {
        _seasonImportingService = seasonImportingService;
        _fileLoader = fileLoader;
        _options = options;
        _logger = logger;
    }

    public async Task Import(string leagueId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(" League {leagueId} import started...", leagueId.ToUpper());

        var league = await GetLeague(leagueId, cancellationToken);
        if (league == null && leagueId != leagueId.ToLower())
        {
            leagueId = leagueId.ToLower();
            league = await GetLeague(leagueId, cancellationToken);
        }
        if (league == null)
            return;

        var leagueScoring = league.Scoring ?? new Scoring(2, 1, 4, 0, 0, 0, 0, Tiebreakers.Default);
        if (_options.Value.Seasons?.Length > 0)
        {
            foreach (var seasonId in _options.Value.Seasons)
                await _seasonImportingService.Import(leagueId, seasonId, leagueScoring, league.MainDivisions, cancellationToken);
        }
        else if (string.IsNullOrEmpty(_options.Value.Season))
        {
            foreach (var seasonId in league.Seasons)
                await _seasonImportingService.Import(leagueId, seasonId, leagueScoring, league.MainDivisions, cancellationToken);
        }
        else
        {
            await _seasonImportingService.Import(leagueId, _options.Value.Season, leagueScoring, league.MainDivisions, cancellationToken);
        }

        _logger.LogInformation(" League {leagueId} imported.", leagueId.ToUpper());
    }

    private async Task<League?> GetLeague(string leagueId, CancellationToken cancellationToken)
    {
        var league = await _fileLoader.LoadLeague(leagueId, cancellationToken);
        if (league == null)
            _logger.LogError(" League {leagueId} cannot be deserialized correctly.", leagueId.ToUpper());
        else
            _logger.LogInformation(" League {leagueId} deserialized correctly.", leagueId.ToUpper());
        return league;
    }
}