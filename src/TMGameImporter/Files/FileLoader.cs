using System.Text.Json;
using System.Text.Json.Serialization;
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
        await DeserializeFile<League>(_pathProvider.GetConfigFilePath(leagueId), true, cancellationToken)
        ?? await DeserializeFile<League>(_pathProvider.GetConfigFilePath(leagueId.ToLower()), true, cancellationToken);

    public async Task<Season?> LoadSeason(string leagueId, string seasonId, CancellationToken cancellationToken) =>
        await DeserializeFile<Season>(_pathProvider.GetConfigFilePath(leagueId, seasonId), false, cancellationToken)
        ?? await DeserializeFile<Season>(_pathProvider.GetConfigFilePath(leagueId.ToLower(), seasonId.ToLower()), false, cancellationToken)
        ?? await DeserializeFile<Season>(_pathProvider.GetConfigFilePath(leagueId.ToLower(), $"s{seasonId.ToLower()}"), false, cancellationToken);

    public async Task<Division?> LoadDivision(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken) =>
        await DeserializeFile<Division>(_pathProvider.GetConfigFilePath(leagueId, seasonId, divisionId), false, cancellationToken) 
        ?? await DeserializeFile<Division>(_pathProvider.GetConfigFilePath(leagueId.ToLower(), seasonId.ToLower(), divisionId.ToLower()), false, cancellationToken)
        ?? await DeserializeFile<Division>(_pathProvider.GetConfigFilePath(leagueId.ToLower(), seasonId.ToLower(), $"d{divisionId.ToLower()}"), false, cancellationToken)
        ?? await DeserializeFile<Division>(_pathProvider.GetConfigFilePath(leagueId.ToLower(), $"s{seasonId.ToLower()}", $"d{divisionId.ToLower()}"), false, cancellationToken);

    public async Task<Game?> LoadGame(int gameId, CancellationToken cancellationToken) =>
        await DeserializeFile<Game>(_pathProvider.GetGamePath(gameId), false, cancellationToken);

    public async Task<Player?> LoadPlayer(string playerName, CancellationToken cancellationToken) =>
        await DeserializeFile<Player>(_pathProvider.GetPlayerFilePath(playerName), false, cancellationToken);

    public async Task<Results?> LoadResults(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken) =>
        await DeserializeFile<Results>(_pathProvider.GetResultsFilePath(leagueId, seasonId, divisionId), false, cancellationToken);

    public bool ExistsResults(string leagueId, string seasonId, string divisionId) =>
        File.Exists(_pathProvider.GetResultsFilePath(leagueId, seasonId, divisionId));

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
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
        try
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, Options, cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}