namespace TMGameImporter.Files;

internal class FileSaver
{
    private readonly PathProvider _pathProvider;

    public FileSaver(PathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    public async Task SaveGameData(string data, uint gameId, CancellationToken cancellationToken) =>
        await File.WriteAllTextAsync(_pathProvider.GetGameDataFilePath(gameId), data, cancellationToken);

    public async Task SaveGameChat(string data, uint gameId, CancellationToken cancellationToken) =>
        await File.WriteAllTextAsync(_pathProvider.GetGameChatPath(gameId), data, cancellationToken);

    public async Task SaveGameLog(string data, uint gameId, CancellationToken cancellationToken) =>
        await File.WriteAllTextAsync(_pathProvider.GetGameLogPath(gameId), data, cancellationToken);

    public async Task SavePlayer(string data, string playerName, CancellationToken cancellationToken) =>
        await File.WriteAllTextAsync(_pathProvider.GetPlayerFilePath(playerName), data, cancellationToken);
}