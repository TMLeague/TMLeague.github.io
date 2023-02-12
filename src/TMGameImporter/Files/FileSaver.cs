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

    public async Task SaveGame(Game game, uint gameId, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetGamePath(gameId);
        await SaveFile(game, cancellationToken, path);
    }

    public async Task SavePlayer(Player player, string playerName, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetPlayerFilePath(playerName);
        await SaveFile(player, cancellationToken, path);
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
        await SaveFile(results, cancellationToken, path);
    }

    public async Task SaveSummary(Summary summary, string leagueId, CancellationToken cancellationToken)
    {
        var path = _pathProvider.GetSummaryFilePath(leagueId);
        await SaveFile(summary, cancellationToken, path);
    }

    private async Task SaveFile(object data, CancellationToken cancellationToken, object path)
    {
        var contents = JsonSerializer.Serialize(data,
            new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals });
        await File.WriteAllTextAsync((string)path, contents, cancellationToken);
        _logger.LogInformation("File saved: {filePath}", path);
    }
}