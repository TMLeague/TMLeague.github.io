using System.Text.Json;
using TMModels;

namespace TMGameImporter.Files;

internal class FileSaver
{
    private readonly PathProvider _pathProvider;

    public FileSaver(PathProvider pathProvider)
    {
        _pathProvider = pathProvider;
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
}