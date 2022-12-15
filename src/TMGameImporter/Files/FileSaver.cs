using Microsoft.Extensions.Logging;
using System.Text.Json;
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

    public async Task SaveGame(Game game, uint gameId, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetGamePath(gameId);
        var data = JsonSerializer.Serialize(game);
        await File.WriteAllTextAsync(path, data, cancellationToken);
    }

    public async Task SavePlayer(Player player, string playerName, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetPlayerFilePath(playerName);
        var data = JsonSerializer.Serialize(player);
        await File.WriteAllTextAsync(path, data, cancellationToken);
    }

    public async Task SavePlayerAvatar(Stream stream, string playerName, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetPlayerAvatarPath(playerName);
        await using var fileStream = File.Create(path);
        stream.Seek(0, SeekOrigin.Begin);
        await stream.CopyToAsync(fileStream, cancellationToken);
    }

    public async Task SaveResults(Results results, string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetResultsFilePath(leagueId, seasonId, divisionId);
        var data = JsonSerializer.Serialize(results);
        await File.WriteAllTextAsync(path, data, cancellationToken);
        _logger.LogInformation("File saved: {filePath}", path);
    }
}