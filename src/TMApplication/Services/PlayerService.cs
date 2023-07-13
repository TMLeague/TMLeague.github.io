using Microsoft.Extensions.Logging;
using TMApplication.Providers;
using TMApplication.ViewModels;

namespace TMApplication.Services;

public class PlayerService
{
    private readonly IDataProvider _dataProvider;
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(IDataProvider dataProvider, ILogger<PlayerService> logger)
    {
        _dataProvider = dataProvider;
        _logger = logger;
    }

    public async Task<PlayerLeagueViewModel?> GetPlayerVm(string leagueId, string playerName, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return null;

        var player = await _dataProvider.GetPlayer(playerName, cancellationToken);
        if (player == null)
            return null;

        _logger.LogInformation($"Name={player.Name}; Leagues={string.Join(", ", player.Leagues.Values.Select(league1 => $"{league1.LeagueId} ({string.Join(",", league1.Results.Select(division => division.SeasonId))})"))}");

        if (!player.Leagues.TryGetValue(leagueId, out var playerLeague))
            return null;

        var seasons = playerLeague.Results
            .Select(division => new PlayerSeasonScoreViewModel(
                division.SeasonId.ToUpper(),
                division.DivisionId.ToUpper(),
                division.Result.TotalPoints,
                division.Result.Wins,
                division.Result.Cla,
                division.Result.Supplies,
                division.Result.PowerTokens,
                division.Result.MinutesPerMove,
                division.Result.Moves,
                division.Result.Houses
                    .Select(houseResult => new PlayerHouseViewModel(
                        houseResult.Game,
                        houseResult.House,
                        houseResult.IsWinner,
                        houseResult.Points,
                        houseResult.BattlePenalty,
                        houseResult.Strongholds,
                        houseResult.Castles,
                        houseResult.Cla,
                        houseResult.Supplies,
                        houseResult.PowerTokens,
                        houseResult.MinutesPerMove,
                        houseResult.Moves,
                        houseResult.Stats))
                    .ToArray(),
                division.Result.PenaltiesPoints,
                division.Result.Stats
            ))
            .ToArray();

        return new PlayerLeagueViewModel(playerName, seasons, player.GeneratedTime);
    }
}