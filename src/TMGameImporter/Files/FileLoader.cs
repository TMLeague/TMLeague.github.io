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
        await DeserializeFile<Home>(_pathProvider.GetConfigFilePath(), true, cancellationToken);

    public async Task<League?> LoadLeague(string leagueId, CancellationToken cancellationToken) =>
        await DeserializeFile<League>(_pathProvider.GetConfigFilePath(leagueId), true, cancellationToken);

    public async Task<Season?> LoadSeason(string leagueId, string seasonId, CancellationToken cancellationToken) =>
        await DeserializeFile<Season>(_pathProvider.GetConfigFilePath(leagueId, seasonId), true, cancellationToken);

    public async Task<Division?> LoadDivision(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken) =>
        await DeserializeFile<Division>(_pathProvider.GetConfigFilePath(leagueId, seasonId, divisionId), true, cancellationToken);

    public async Task<Game?> LoadGame(uint gameId, CancellationToken cancellationToken) =>
        await DeserializeFile<Game>(_pathProvider.GetGamePath(gameId), false, cancellationToken);

    public async Task<Player?> LoadPlayer(string playerName, CancellationToken cancellationToken) =>
        await DeserializeFile<Player>(_pathProvider.GetPlayerFilePath(playerName), false, cancellationToken);

    public bool ExistsResults(string leagueId, string seasonId, string divisionId) =>
        File.Exists(_pathProvider.GetResultsFilePath(leagueId, seasonId, divisionId));

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static async Task<T?> DeserializeFile<T>(string path, bool throwErrorOnNotFound, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            if (throwErrorOnNotFound)
                throw new FileNotFoundException($"File not found! It should be located here: {path}");
            return default;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, Options, cancellationToken: cancellationToken);
    }
}