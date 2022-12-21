using TMApplication.Providers;
using TMApplication.ViewModels;
using TMModels;

namespace TMApplication.Services;

public class DivisionService
{
    private readonly IDataProvider _dataProvider;
    private readonly GameService _gameService;

    public DivisionService(IDataProvider dataProvider, GameService gameService)
    {
        _dataProvider = dataProvider;
        _gameService = gameService;
    }

    public async Task<DivisionSummaryViewModel?> GetDivisionSummaryVm(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken = default)
    {
        var division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);

        var games = new List<GameSummaryViewModel>();

        if (division == null)
            return new DivisionSummaryViewModel(leagueId, seasonId, divisionId, null, 0, games, null, DateTimeOffset.Now);

        var progress = 0.0;
        foreach (var gameId in division.Games)
        {
            var game = await _gameService.GetGameSummaryVm(gameId, cancellationToken);
            games.Add(game);
            progress += game.Progress;
        }

        progress /= games.Count;

        var winnerPlayerName = await GetWinner(leagueId, seasonId, divisionId, division, cancellationToken);

        var lastModifiedDate = games.Max(game => game?.GeneratedTime ?? DateTimeOffset.MinValue);
        return new DivisionSummaryViewModel(leagueId, seasonId, divisionId, division?.Name, progress, games, winnerPlayerName, lastModifiedDate.ToLocalTime());
    }

    private async Task<string?> GetWinner(string leagueId, string seasonId, string divisionId,
        Division division, CancellationToken cancellationToken)
    {
        if (!division.IsFinished)
            return null;

        var results = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);
        return results?.Players.First().Player;
    }

    public async Task<DivisionViewModel?> GetDivisionVm(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return null;

        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
            return null;

        var division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);
        if (division == null)
            return null;

        var results = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);
        return new DivisionViewModel(league.Name, season.Name, division.Name, division.Judge, division.IsFinished, division.WinnerTitle,
            (results?.Players.Select(GetPlayerVm) ??
             division.Players.Select(s => new DivisionPlayerViewModel(s))).ToArray());
    }

    private static DivisionPlayerViewModel GetPlayerVm(PlayerResult playerResult) => new(
        playerResult.Player,
        playerResult.TotalPoints,
        playerResult.Wins,
        playerResult.Cla,
        playerResult.Supplies,
        playerResult.PowerTokens,
        playerResult.MinutesPerMove,
        playerResult.Moves,
        playerResult.Houses.Select(GetPlayerHouseVm).ToArray(),
        playerResult.PenaltiesPoints,
        playerResult.PenaltiesDetails.Select(GetPlayerPenaltyVm).ToArray());

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
        houseResult.Moves);

    private static PlayerPenaltyViewModel GetPlayerPenaltyVm(PlayerPenalty playerPenalty) => new(
        playerPenalty.Game,
        playerPenalty.Points,
        playerPenalty.Details);
}