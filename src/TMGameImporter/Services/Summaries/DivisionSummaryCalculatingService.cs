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

        var results = await _fileLoader.LoadResults(leagueId, seasonId, divisionId, cancellationToken);
        if (results == null)
        {
            _logger.LogError(
                "   Division results {leagueId}/{seasonId}/{divisionId} cannot be deserialized correctly.",
                leagueId, seasonId, divisionId);
            return null;
        }

        var players = results.Players.Select(playerResult => new SummaryPlayerScore(
            playerResult.Player,
            GetPlayerSummaryScore(playerResult),
            GetPlayerSummaryScore(playerResult)))
            .ToArray();

        _logger.LogInformation(
            "   Division {leagueId}/{seasonId}/{divisionId} summary calculated.",
            leagueId.ToUpper(), seasonId.ToUpper(), divisionId.ToUpper());

        return new SummaryDivision(divisionId, players);
    }

    private static SummaryScore GetPlayerSummaryScore(PlayerResult playerResult) => new(
        playerResult.TotalPoints,
        playerResult.Wins,
        playerResult.Cla,
        playerResult.Supplies,
        playerResult.PowerTokens,
        playerResult.MinutesPerMove,
        playerResult.Moves,
        GetSummaryHouseScore(playerResult.Houses),
        playerResult.PenaltiesPoints);

    private static SummaryHouseScore[] GetSummaryHouseScore(IEnumerable<HouseResult> houses) =>
        houses.Select(houseResult => new SummaryHouseScore(
                houseResult.House,
                houseResult.Points))
            .ToArray();
}