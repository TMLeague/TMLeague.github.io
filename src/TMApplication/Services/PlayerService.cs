using TMApplication.Providers;
using TMApplication.ViewModels;
using TMModels;

namespace TMApplication.Services;

public class PlayerService
{
    private readonly IDataProvider _dataProvider;

    public PlayerService(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<PlayerViewModel?> GetPlayerVm(string playerName, CancellationToken cancellationToken = default)
    {
        var player = await _dataProvider.GetPlayer(playerName, cancellationToken);
        if (player == null)
            return null;

        var leagues = new List<PlayerLeagueViewModel>();

        foreach (var playerLeague in player.Leagues.Values)
        {
            var playerLeagueVm = await GetPlayerLeagueVm(playerLeague, cancellationToken);
            leagues.Add(playerLeagueVm);
        }

        return new PlayerViewModel(playerName, leagues.ToArray(), player.GeneratedTime);
    }

    private async Task<PlayerLeagueViewModel> GetPlayerLeagueVm(PlayerLeague playerLeague, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(playerLeague.LeagueId, cancellationToken);

        return new PlayerLeagueViewModel(
            playerLeague.LeagueId,
            league?.Name ?? string.Empty,
            playerLeague.Results.Select(GetPlayerSeasonScoreVm).ToArray());
    }

    public async Task<Dictionary<string, PlayersInteractions>?> GetPlayerInteractionsVm(string playerName, CancellationToken cancellationToken = default)
    {
        var player = await _dataProvider.GetPlayer(playerName, cancellationToken);
        if (player == null)
            return null;

        var interactions = new Dictionary<string, PlayersInteractions>();
        foreach (var leagueId in player.Leagues.Keys)
        {
            var playerInteractions = await GetPlayerLeagueInteractionsVm(playerName, leagueId, cancellationToken);
            if (playerInteractions != null)
                interactions[leagueId] = playerInteractions;
        }

        return interactions;
    }

    private async Task<PlayersInteractions?> GetPlayerLeagueInteractionsVm(string playerName, string leagueId, CancellationToken cancellationToken = default)
    {
        var interactions = await _dataProvider.GetLeagueInteractions(leagueId, cancellationToken);

        return interactions?.Players.TryGetValue(playerName, out var playersInteractions) ?? false ? playersInteractions : null;
    }

    private static PlayerSeasonScoreViewModel GetPlayerSeasonScoreVm(PlayerDivision division) => new(
        division.SeasonId,
        division.DivisionId,
        division.Result.Position,
        division.Result.TotalPoints,
        division.Result.Wins,
        division.Result.Cla,
        division.Result.Supplies,
        division.Result.PowerTokens,
        division.Result.MinutesPerMove,
        division.Result.Moves,
        division.Result.Houses.Select(GetPlayerHouseVm).ToArray(),
        division.Result.PenaltiesPoints,
        division.Result.Stats);

    private static PlayerHouseViewModel GetPlayerHouseVm(HouseResult houseResult) => new(
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
        houseResult.Stats);
}