using System.Text.Json;
using TMModels;

namespace TMGameImporter.Files;

internal class FileLoader
{
    private readonly PathProvider _pathProvider;

    public FileLoader(PathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    public async Task<Home?> LoadHome(CancellationToken cancellationToken) =>
        await DeserializeFile<Home>(_pathProvider.GetConfigFilePath(), cancellationToken);

    public async Task<League?> LoadLeague(string leagueId, CancellationToken cancellationToken) =>
        await DeserializeFile<League>(_pathProvider.GetConfigFilePath(leagueId), cancellationToken);

    public async Task<Season?> LoadSeason(string leagueId, string seasonId, CancellationToken cancellationToken) =>
        await DeserializeFile<Season>(_pathProvider.GetConfigFilePath(leagueId, seasonId), cancellationToken);

    public async Task<Division?> LoadDivision(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken) =>
        await DeserializeFile<Division>(_pathProvider.GetConfigFilePath(leagueId, seasonId, divisionId), cancellationToken);

    private static async Task<T?> DeserializeFile<T>(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found! It should be located here: {path}");

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
    }
}