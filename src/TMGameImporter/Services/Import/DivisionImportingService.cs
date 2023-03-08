using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMGameImporter.Configuration;
using TMGameImporter.Files;
using TMModels;

namespace TMGameImporter.Services.Import;

internal class DivisionImportingService
{
    private readonly GameImportingService _gameImportingService;
    private readonly PlayerImportingService _playerImportingService;
    private readonly FileLoader _fileLoader;
    private readonly FileSaver _fileSaver;
    private readonly IOptions<ImporterOptions> _options;
    private readonly ILogger<DivisionImportingService> _logger;

    public DivisionImportingService(GameImportingService gameImportingService,
        PlayerImportingService playerImportingService, FileLoader fileLoader,
        FileSaver fileSaver, IOptions<ImporterOptions> options, ILogger<DivisionImportingService> logger)
    {
        _gameImportingService = gameImportingService;
        _playerImportingService = playerImportingService;
        _fileLoader = fileLoader;
        _fileSaver = fileSaver;
        _options = options;
        _logger = logger;
    }

    public async Task Import(string leagueId, string seasonId, string divisionId,
        Scoring scoring, CancellationToken cancellationToken)
    {
        _logger.LogInformation("   Division {leagueId}/{seasonId}/{divisionId} import started...",
            leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());

        var division = await _fileLoader.LoadDivision(leagueId, seasonId, divisionId, cancellationToken);
        if (division == null)
        {
            _logger.LogError("   Division {leagueId}/{seasonId}/{divisionId} cannot be deserialized correctly.",
                leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
            return;
        }

        if (division.IsFinished && _fileLoader.ExistsResults(leagueId, seasonId, divisionId) && !_options.Value.FetchFinishedDivisions)
        {
            _logger.LogInformation("   Division {leagueId}/{seasonId}/{divisionId} is already fetched and is finished.",
                leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
            return;
        }

        //foreach (var playerName in division.Enemies)
        //    await _playerImportingService.Import(playerName, cancellationToken);
        var games = division.Games.AsParallel()
            .Select(gameId => _gameImportingService.Import(gameId, cancellationToken))
            .Select(task => task.Result)
            .OfType<Game>()
            .ToArray();

        var playerResults = division.Players
            .Select(playerName => GetPlayerResults(playerName, games, scoring, division))
            .ToArray();
        Array.Sort(playerResults, (p1, p2) => -Compare(p1, p2, scoring.Tiebreakers));
        var results = new Results(playerResults, DateTimeOffset.UtcNow);
        await _fileSaver.SaveResults(results, leagueId, seasonId, divisionId, cancellationToken);

        _logger.LogInformation("   Division {leagueId}/{seasonId}/{divisionId} imported.",
            leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
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
            penalties);
    }

    private static HouseResult[] GetHouses(string playerName, Replacement[] divisionReplacements, IEnumerable<Game> games, Scoring scoring) =>
        games
            .Select(game => GetPlayerHouse(playerName, divisionReplacements, game))
            .Where(tuple => tuple.HouseScore != null)
            .Select(tuple => GetHouseResult(tuple.Game, tuple.HouseScore!, scoring))
            .ToArray();

    private static (Game Game, HouseScore? HouseScore) GetPlayerHouse(string playerName, Replacement[] divisionReplacements, Game game) => (game,
        game.Houses
            .FirstOrDefault(houseScore => houseScore.Player == playerName ||
                                          divisionReplacements.Contains(new Replacement(playerName, houseScore.Player, game.Id))));

    private static HouseResult GetHouseResult(Game game, HouseScore houseScore, Scoring scoring)
    {
        var isWinner = game.Houses.First().House == houseScore.House;
        return new HouseResult(game.Id,
            houseScore.House,
            isWinner,
            GetPointsForGame(houseScore, isWinner, scoring),
            GetBattlePenalty(game, houseScore, scoring),
            houseScore.Strongholds,
            houseScore.Castles,
            houseScore.Cla,
            houseScore.Supplies,
            houseScore.PowerTokens,
            houseScore.MinutesPerMove,
            houseScore.Moves);
    }

    private static double GetPointsForGame(HouseScore houseScore, bool isWinner, Scoring scoring) =>
        scoring.PointsPerStronghold * houseScore.Strongholds +
        scoring.PointsPerCastle * houseScore.Castles +
        (isWinner ? scoring.PointsPerWin : 0);

    private static int GetBattlePenalty(Game game, HouseScore houseScore, Scoring scoring)
    {
        if (game.Turn < 10)
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
            .Select(houseResult => new PlayerPenalty(houseResult.Game, houseResult.BattlePenalty, "for not enough battles before 10th round"));
        return (divisionPenalties?.Concat(battlePenalties) ?? battlePenalties).ToArray();
    }

    private static int Compare(PlayerResult p1, PlayerResult p2, IEnumerable<Tiebreaker> tiebreakers)
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
                    if (p1.PenaltiesPoints == p2.PenaltiesPoints)
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
                    return p1.MinutesPerMove.CompareTo(p2.MinutesPerMove);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return 0;
    }
}