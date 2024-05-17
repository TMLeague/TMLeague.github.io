using Microsoft.Extensions.Logging;
using TMGameImporter.Files;
using TMModels;

namespace TMGameImporter.Services.Summaries;

internal class DivisionSummaryCalculatingService
{
    private readonly FileLoader _fileLoader;
    private readonly ILogger<LeagueSummaryCalculatingService> _logger;

    public DivisionSummaryCalculatingService(FileLoader fileLoader, ILogger<LeagueSummaryCalculatingService> logger)
    {
        _fileLoader = fileLoader;
        _logger = logger;
    }

    public async Task<SummaryDivision?> Calculate(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "   Division {leagueId}/{seasonId}/{divisionId} summary calculation started...",
            leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());

        var division = await _fileLoader.LoadDivision(leagueId, seasonId, divisionId, cancellationToken);
        if (division == null)
        {
            _logger.LogError(
                "   Division {leagueId}/{seasonId}/{divisionId} cannot be deserialized correctly.",
                leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
            return null;
        }

        if (!division.IsFinished)
        {
            _logger.LogWarning(
                "   Division {leagueId}/{seasonId}/{divisionId} is not finished.",
                leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
            return null;
        }

        var results = await _fileLoader.LoadResults(leagueId, seasonId, divisionId, cancellationToken);
        if (results == null)
        {
            _logger.LogError(
                "   Division results {leagueId}/{seasonId}/{divisionId} cannot be deserialized correctly.",
                leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());
            return null;
        }

        var disqualified = division.Penalties?
            .Where(penalty => penalty.Disqualification)
            .Select(penalty => penalty.Player)
            .ToHashSet() ?? new HashSet<string>();

        var players = results.Players
            .Where(result => !disqualified.Contains(result.Player))
            .Select((playerResult, idx) => new SummaryPlayerScore(
                playerResult.Player,
                GetPlayerSummaryScore(playerResult, idx),
                GetPlayerSummaryScore(playerResult, idx)))
            .ToArray();

        var houses = results.Players
            .SelectMany(playerResult => playerResult.Houses)
            .Select(houseResult => new SummaryHouseScore(
                houseResult.House,
                GetHouseSummaryScore(houseResult),
                GetHouseSummaryScore(houseResult)))
            .GroupBy(score => score.House)
            .Select(scores =>
                scores.Aggregate(
                    new SummaryHouseScore(scores.Key),
                    (score1, score2) => score1 + score2))
            .ToArray();

        _logger.LogInformation(
            "   Division {leagueId}/{seasonId}/{divisionId} summary calculated.",
            leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());

        return new SummaryDivision(divisionId, division.Name, players, houses);
    }

    private static SummaryPlayerScoreDetails GetPlayerSummaryScore(PlayerResult playerResult, int position) => new(
        playerResult.TotalPoints,
        playerResult.Wins,
        playerResult.Cla,
        playerResult.Supplies,
        playerResult.PowerTokens,
        playerResult.MinutesPerMove,
        playerResult.Moves,
        GetSummaryHouseScore(playerResult.Houses),
        playerResult.PenaltiesPoints,
        position + 1,
        playerResult.Stats);

    private static SummaryHouseScoreDetails GetHouseSummaryScore(HouseResult houseResult) => new(
        houseResult.Points,
        houseResult.IsWinner ? 1 : 0,
        houseResult.Cla,
        houseResult.Supplies,
        houseResult.PowerTokens,
        houseResult.Moves,
        houseResult.Stats);

    private static HousePoints[] GetSummaryHouseScore(IEnumerable<HouseResult> houses) =>
        houses.Select(houseResult => new HousePoints(
                houseResult.House,
                houseResult.Points))
            .ToArray();
}