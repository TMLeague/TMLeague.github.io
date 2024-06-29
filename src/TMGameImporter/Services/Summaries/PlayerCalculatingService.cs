using Microsoft.Extensions.Logging;
using TMGameImporter.Files;
using TMModels;

namespace TMGameImporter.Services.Summaries;

internal class PlayerCalculatingService
{
    private readonly FileLoader _fileLoader;
    private readonly FileSaver _fileSaver;
    private readonly ILogger<PlayerCalculatingService> _logger;

    public PlayerCalculatingService(FileLoader fileLoader, FileSaver fileSaver, ILogger<PlayerCalculatingService> logger)
    {
        _fileLoader = fileLoader;
        _fileSaver = fileSaver;
        _logger = logger;
    }

    public async Task Calculate(CancellationToken cancellationToken = default)
    {
        var home = await _fileLoader.LoadHome(cancellationToken);
        if (home == null)
        {
            _logger.LogError("Home cannot be deserialized correctly.");
            return;
        }

        var players = new Dictionary<string, Player>();

        foreach (var leagueId in home.Leagues)
            await AddLeague(players, leagueId, cancellationToken);

        foreach (var player in players.Values)
            await _fileSaver.SavePlayer(player, player.Name, cancellationToken);
    }

    private async Task AddLeague(IDictionary<string, Player> players, string leagueId, CancellationToken cancellationToken)
    {
        var league = await _fileLoader.LoadLeague(leagueId, cancellationToken);
        if (league == null)
        {
            _logger.LogError(
                "League {leagueId} cannot be deserialized correctly.",
                leagueId.ToUpper());
            return;
        }
        foreach (var seasonId in league.Seasons)
            await AddSeason(players, leagueId, seasonId, cancellationToken);
    }

    private async Task AddSeason(IDictionary<string, Player> players, string leagueId, string seasonId, CancellationToken cancellationToken)
    {
        var season = await _fileLoader.LoadSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
        {
            _logger.LogError(
                "Season {leagueId}/{seasonId} cannot be deserialized correctly.",
                leagueId.ToUpper(), seasonId.ToUpper());
            return;
        }
        foreach (var divisionId in season.Divisions)
            await AddDivision(players, leagueId, seasonId, divisionId, cancellationToken);
    }

    private async Task AddDivision(IDictionary<string, Player> players, string leagueId, string seasonId, string divisionId,
        CancellationToken cancellationToken)
    {
        var division = await _fileLoader.LoadResults(leagueId, seasonId, divisionId, cancellationToken);
        if (division == null)
        {
            _logger.LogError(
                "Division results {leagueId}/{seasonId}/{divisionId} cannot be deserialized correctly.",
                leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
            return;
        }
        foreach (var playerResult in division.Players)
            AddPlayerResults(players, leagueId, seasonId, divisionId, playerResult, division.GeneratedTime);
    }

    private static void AddPlayerResults(IDictionary<string, Player> players, string leagueId, string seasonId,
        string divisionId, PlayerResult playerResult, DateTimeOffset generatedTime)
    {
        if (!players.TryGetValue(playerResult.Player, out var player))
        {
            player = new Player(playerResult.Player, generatedTime);
            players.Add(playerResult.Player, player);
        }

        if (!player.Leagues.TryGetValue(leagueId, out var playerLeague))
        {
            playerLeague = new PlayerLeague(leagueId, []);
            player.Leagues.Add(leagueId, playerLeague);
            if (generatedTime > player.GeneratedTime)
                player.GeneratedTime = generatedTime;
        }

        playerLeague.AddDivision(seasonId, divisionId, playerResult);
    }
}