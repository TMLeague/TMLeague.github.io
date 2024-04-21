using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using TMModels;

namespace TMGameImporter.Files;

internal class FileSaver
{
    private readonly PathProvider _pathProvider;
    private readonly ILogger<FileSaver> _logger;

    public FileSaver(PathProvider pathProvider, ILogger<FileSaver> logger)
    {
        _pathProvider = pathProvider;
        _logger = logger;
    }

    public async Task SaveGame(Game game, int gameId, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetGamePath(gameId);
        await SaveFile(game, path, cancellationToken);
    }

    public async Task SavePlayer(Player player, string playerName, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetPlayerFilePath(playerName);
        await SaveFile(player, path, cancellationToken);
    }

    public async Task SaveResults(Results results, string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetResultsFilePath(leagueId, seasonId, divisionId);
        await SaveFile(results, path, cancellationToken);
    }

    public async Task SaveSummary(Summary summary, string leagueId, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetSummaryFilePath(leagueId);
        await SaveFile(summary, path, cancellationToken);
    }

    public async Task SaveLeagueInteractions(TotalInteractions interactions, string leagueId, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetLeagueInteractions(leagueId);
        await SaveFile(interactions, path, cancellationToken);
    }

    public async Task SaveDivisionInteractions(TotalInteractions interactions, string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetDivisionInteractions(leagueId, seasonId, divisionId);
        await SaveFile(interactions, path, cancellationToken);
    }

    private async Task SaveFile(object data, object path, CancellationToken cancellationToken)
    {
        var contents = JsonSerializer.Serialize(data,
            new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                PropertyNameCaseInsensitive = true
            });
        await File.WriteAllTextAsync((string)path, contents, cancellationToken);
        _logger.LogInformation("File saved: {filePath}", path);
    }
}