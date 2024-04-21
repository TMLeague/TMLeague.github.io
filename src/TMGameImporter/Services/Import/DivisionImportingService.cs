using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMGameImporter.Configuration;
using TMGameImporter.Files;
using TMModels;

namespace TMGameImporter.Services.Import;

internal class DivisionImportingService
{
    private readonly GameImportingService _gameImportingService;
    private readonly FileLoader _fileLoader;
    private readonly FileSaver _fileSaver;
    private readonly IOptions<ImporterOptions> _options;
    private readonly ILogger<DivisionImportingService> _logger;

    public DivisionImportingService(GameImportingService gameImportingService, FileLoader fileLoader,
        FileSaver fileSaver, IOptions<ImporterOptions> options, ILogger<DivisionImportingService> logger)
    {
        _gameImportingService = gameImportingService;
        _fileLoader = fileLoader;
        _fileSaver = fileSaver;
        _options = options;
        _logger = logger;
    }

    public async Task Import(string leagueId, string seasonId, string divisionId,
        Scoring scoring, int? leaguePromotions, int? leagueRelegations, CancellationToken cancellationToken)
    {
        _logger.LogInformation("   Division {leagueId}/{seasonId}/{divisionId} import started...",
            leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());

        var division = await GetDivision(leagueId, seasonId, divisionId, cancellationToken);
        if (division == null && divisionId != divisionId.ToLower())
        {
            divisionId = divisionId.ToLower();
            division = await GetDivision(leagueId, seasonId, divisionId, cancellationToken);
        }
        if (division == null)
        {
            divisionId = $"d{divisionId.ToLower()}";
            division = await GetDivision(leagueId, seasonId, divisionId, cancellationToken);
        }
        if (division == null)
            return;

        var oldResults = await _fileLoader.LoadResults(leagueId, seasonId, divisionId, cancellationToken);
        if (oldResults != null)
        {
            if (oldResults.IsCreatedManually)
            {
                _logger.LogInformation("   Division {leagueId}/{seasonId}/{divisionId} is created manually and can't be overriden.",
                    leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
                return;
            }

            if (oldResults.IsFinished && !_options.Value.FetchFinishedDivisions)
            {
                _logger.LogInformation("   Division {leagueId}/{seasonId}/{divisionId} is already fetched and is finished.",
                    leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
                return;
            }
        }

        //foreach (var playerName in division.Enemies)
        //    await _playerCalculatingService.Import(playerName, cancellationToken);
        var games = division.Games
            .OfType<int>()
            .Select(gameId => _gameImportingService.Import(gameId, cancellationToken))
            .Select(task => task.Result)
            .OfType<Game>()
            .ToList();

        foreach (var gameId in division.Games)
        {
            if (gameId == null || games.Any(game => game.Id == gameId))
                continue;

            var oldGame = await _fileLoader.LoadGame(gameId.Value, cancellationToken);
            if (oldGame != null)
                games.Add(oldGame);
        }

        var playerResults = division.Players
            .Select(playerName => GetPlayerResults(playerName, games, scoring, division))
            .ToArray();
        Array.Sort(playerResults, (p1, p2) => -Compare(p1, p2, scoring.Tiebreakers));
        UpdatePlayersPositions(playerResults, division.Promotions ?? leaguePromotions ?? 0, division.Relegations ?? leagueRelegations ?? 0);

        var results = new Results(playerResults, games.Any() ? games.Max(game => game.GeneratedTime) : oldResults?.GeneratedTime ?? DateTimeOffset.UtcNow, division.IsFinished);
        await _fileSaver.SaveResults(results, leagueId, seasonId, divisionId, cancellationToken);

        var interactions = games.Aggregate(new TotalInteractions(), (totalInteractions, game) => totalInteractions + game);
        await _fileSaver.SaveDivisionInteractions(interactions, leagueId, seasonId, divisionId, cancellationToken);

        _logger.LogInformation("   Division {leagueId}/{seasonId}/{divisionId} imported.",
            leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
    }

    private async Task<Division?> GetDivision(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken)
    {
        var division = await _fileLoader.LoadDivision(leagueId, seasonId, divisionId, cancellationToken);
        if (division == null)
            _logger.LogError("   Division {leagueId}/{seasonId}/{divisionId} cannot be deserialized correctly.",
                leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
        else
            _logger.LogInformation("   Division {leagueId}/{seasonId}/{divisionId} deserialized correctly.",
                leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());

        return division;
    }

    private static PlayerResult GetPlayerResults(string playerName, IEnumerable<Game> games, Scoring scoring, Division division)
    {
        var houseResults = GetHouses(playerName, division.Replacements ?? Array.Empty<Replacement>(), games, scoring);
        var penalties = GetPenalties(playerName, division, houseResults);
        var penaltiesPoints = penalties.Sum(penalty => penalty.Points);
        var moves = houseResults.Sum(houseResult => houseResult.Moves);
        var minutesPerMove = moves == 0 ?
            0 : houseResults.Sum(houseResult => houseResult.MinutesPerMove * houseResult.Moves) / moves;
        return new PlayerResult(playerName,
            houseResults.Sum(houseResult => houseResult.Points) - penaltiesPoints,
            houseResults.Count(houseResult => houseResult.IsWinner),
            houseResults.Sum(houseResult => houseResult.Cla),
            houseResults.Sum(houseResult => houseResult.Supplies),
            houseResults.Sum(houseResult => houseResult.PowerTokens),
            minutesPerMove,
            moves,
            houseResults,
            penaltiesPoints,
            penalties,
            houseResults.Aggregate(new Stats(), (sum, results) => sum + results.Stats));
    }

    private static HouseResult[] GetHouses(string playerName, Replacement[] replacements, IEnumerable<Game> games, Scoring scoring) =>
        games
            .Select(game => GetPlayerHouse(playerName, replacements, game))
            .Where(tuple => tuple.HouseScore != null)
            .Select(tuple => GetHouseResult(tuple.Game, tuple.HouseScore!, scoring))
            .ToArray();

    private static (Game Game, HouseScore? HouseScore) GetPlayerHouse(string playerName, Replacement[] replacements, Game game) => (game,
        game.Houses
            .FirstOrDefault(houseScore => playerName == GetHousePlayer(houseScore, replacements, game)));

    private static string GetHousePlayer(HouseScore houseScore, IEnumerable<Replacement> replacements, Game game)
    {
        var replacement = replacements.FirstOrDefault(replacement =>
            replacement.Game == game.Id && replacement.To == houseScore.Player);
        return replacement == null ? houseScore.Player : replacement.From;
    }

    private static HouseResult GetHouseResult(Game game, HouseScore houseScore, Scoring scoring)
    {
        var position = Array.IndexOf(game.Houses, houseScore) + 1;
        return new HouseResult(game.Id,
            houseScore.House,
            position == 1,
            GetPointsForGame(houseScore, position, scoring),
            GetBattlePenalty(houseScore, scoring),
            houseScore.Strongholds,
            houseScore.Castles,
            houseScore.Cla,
            houseScore.Supplies,
            houseScore.PowerTokens,
            houseScore.MinutesPerMove,
            houseScore.Moves,
            houseScore.Stats);
    }

    private static double GetPointsForGame(HouseScore houseScore, int position, Scoring scoring) =>
        scoring.PointsPerStronghold * houseScore.Strongholds +
        scoring.PointsPerCastle * houseScore.Castles +
        (position == 1 ? scoring.PointsPerWin : 0) +
        (IsCleanWin(houseScore) ? scoring.PointsPerClearWin : 0) +
        (position == 2 ? scoring.PointsPer2ndPlace : 0) +
        (position == 3 ? scoring.PointsPer3rdPlace : 0);

    private static bool IsCleanWin(HouseScore houseScore) =>
        houseScore.Castles + houseScore.Strongholds >= 7;

    private static int GetBattlePenalty(HouseScore houseScore, Scoring scoring)
    {
        if (houseScore.Turn < 10 || houseScore.BattlesInTurn.Length < 10)
            return 0;

        var battlesBefore10ThTurn = houseScore.BattlesInTurn[..9].Sum(battlesInTurn => battlesInTurn);
        return Math.Max(0, scoring.RequiredBattlesBefore10thTurn - battlesBefore10ThTurn);
    }

    private static PlayerPenalty[] GetPenalties(string playerName, Division division, IEnumerable<HouseResult> houseResults)
    {
        var divisionPenalties = division.Penalties?
            .Where(penalty => penalty.Player == playerName)
            .Select(penalty => new PlayerPenalty(penalty.Game, penalty.Points, penalty.Details));
        var battlePenalties = houseResults
            .Where(houseResult => houseResult.BattlePenalty > 0)
            .Select(houseResult => new PlayerPenalty(houseResult.Game, houseResult.BattlePenalty, Penalty.BattlePenalty));
        return (divisionPenalties?.Concat(battlePenalties) ?? battlePenalties).ToArray();
    }

    private static void UpdatePlayersPositions(IReadOnlyList<PlayerResult> playerResults, int promotions, int relegations)
    {
        var playersCount = playerResults.Count;
        for (var i = 0; i < playersCount; i++)
        {
            var playerResult = playerResults[i];
            playerResult.Position = i + 1;
            if (playerResult.Position <= promotions)
                playerResult.IsPromoted = true;
            if (playerResult.Position > playersCount - relegations)
                playerResult.IsRelegated = true;
        }
    }

    internal static int Compare(PlayerResult p1, PlayerResult p2, IEnumerable<Tiebreaker> tiebreakers)
    {
        if (p1.TotalPoints != p2.TotalPoints)
            return p1.TotalPoints.CompareTo(p2.TotalPoints);

        foreach (var tiebreaker in tiebreakers)
        {
            switch (tiebreaker)
            {
                case Tiebreaker.Wins:
                    if (p1.Wins == p2.Wins)
                        continue;
                    return p1.Wins - p2.Wins;

                case Tiebreaker.Penalties:
                    if (Math.Abs(p1.PenaltiesPoints - p2.PenaltiesPoints) < 0.0001)
                        continue;
                    return p2.PenaltiesPoints.CompareTo(p1.PenaltiesPoints);

                case Tiebreaker.Cla:
                    if (p1.Cla == p2.Cla)
                        continue;
                    return p1.Cla - p2.Cla;

                case Tiebreaker.Supplies:
                    if (p1.Supplies == p2.Supplies)
                        continue;
                    return p1.Supplies - p2.Supplies;

                case Tiebreaker.PowerTokens:
                    if (p1.PowerTokens == p2.PowerTokens)
                        continue;
                    return p1.PowerTokens - p2.PowerTokens;

                case Tiebreaker.MinutesPerMove:
                    if (p1.MinutesPerMove.CompareTo(p2.MinutesPerMove) == 0)
                        continue;
                    return p2.MinutesPerMove.CompareTo(p1.MinutesPerMove);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return 0;
    }
}