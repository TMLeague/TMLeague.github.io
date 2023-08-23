using System.Runtime.CompilerServices;
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

    public async Task<LeagueDivisionSummaryViewModel?> GetDivisionSummaryVm(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken = default)
    {
        var division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);

        var games = new List<LeagueGameSummaryViewModel>();

        if (division == null)
            return new LeagueDivisionSummaryViewModel(leagueId, seasonId, divisionId, null, 0, games, null);

        var progress = 0.0;
        foreach (var gameId in division.Games)
        {
            var game = await _gameService.GetGameSummaryVm(gameId, cancellationToken);
            games.Add(game);
            progress += game.Progress;
        }

        if (games.Count > 0)
            progress /= games.Count;

        var winnerPlayerName = await GetWinner(leagueId, seasonId, divisionId, division, cancellationToken);

        return new LeagueDivisionSummaryViewModel(leagueId, seasonId, divisionId, division?.Name, progress, games, winnerPlayerName);
    }

    private async Task<string?> GetWinner(string leagueId, string seasonId, string divisionId,
        Division division, CancellationToken cancellationToken)
    {
        if (!division.IsFinished)
            return null;

        var results = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);
        return results?.Players.First().Player;
    }

    public async Task<DivisionViewModel?> GetDivisionVm(string leagueId, string seasonId, string divisionId,
        CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return null;

        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
            return null;

        var division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);
        if (division == null && season.Divisions.Length > 0)
        {
            divisionId = season.Divisions.Last();
            division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);
        }

        if (division == null)
            return null;

        var results = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);

        var players = (results?.Players.Select(GetPlayerVm) ??
                       division.Players.Select(s => new DivisionPlayerViewModel(s))).ToArray();
        var messages = await GetMessages(leagueId, seasonId, divisionId, division, cancellationToken);

        var games = await GetDivisionGames(division, cancellationToken).ToArrayAsync(cancellationToken);
        return new DivisionViewModel(
            league.Name,
            season.Name,
            division.Name,
            league.JudgeTitle ?? "Judge",
            division.Judge,
            division.IsFinished,
            division.WinnerTitle,
            players,
            games,
            league.Scoring?.Tiebreakers ?? Tiebreakers.Default,
            messages,
            results?.GeneratedTime,
            league.GetSeasonNavigation(seasonId),
            season.GetDivisionNavigation(divisionId));
    }

    private async IAsyncEnumerable<DivisionGameViewModel?> GetDivisionGames(Division division, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var gameId in division.Games)
        {
            if (gameId == null)
            {
                yield return null;
            }
            else
            {
                var game = await _dataProvider.GetGame(gameId.Value, cancellationToken);
                yield return game == null ?
                    null :
                    new DivisionGameViewModel(game.Id, game.Name, game.Turn, game.Progress, game.IsFinished, game.IsStalling, game.IsCreatedManually);
            }
        }
    }

    private async Task<NotificationMessage[]> GetMessages(string leagueId, string seasonId, string divisionId,
        Division division, CancellationToken cancellationToken = default)
    {
        if (division.IsFinished)
            return Array.Empty<NotificationMessage>();

        var messages = new List<NotificationMessage>();

        foreach (var (gameId, gameIdx) in division.Games.Select((g, i) => (g, i)))
        {
            var game = gameId == null ?
                null :
                await _dataProvider.GetGame(gameId.Value, cancellationToken);
            if (game == null || game.IsStalling || game.IsFinished)
                continue;
            foreach (var houseScore in game.Houses)
            {
                if (houseScore.MinutesPerMove >= 500)
                    messages.Add(new NotificationMessage(
                        NotificationLevel.Critical,
                        $"{houseScore.House} in <a href=\"{RouteProvider.GetGameRoute(leagueId, game.Id)}\" class=\"text-inherit\">G{gameIdx + 1}</a> has {Math.Round(houseScore.MinutesPerMove)} mpm."));
                else if (houseScore.MinutesPerMove >= 300)
                    messages.Add(new NotificationMessage(
                        NotificationLevel.Warning,
                        $"{houseScore.House} in <a href=\"{RouteProvider.GetGameRoute(leagueId, game.Id)}\" class=\"text-inherit\">G{gameIdx + 1}</a> has {Math.Round(houseScore.MinutesPerMove)} mpm."));
            }
        }

        return messages.OrderBy(message => -(int)message.Level).ToArray();
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
        playerResult.PenaltiesDetails.Select(GetPlayerPenaltyVm).ToArray(),
        playerResult.Stats,
        playerResult.IsPromoted,
        playerResult.IsRelegated);

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

    private static PlayerPenaltyViewModel GetPlayerPenaltyVm(PlayerPenalty playerPenalty) => new(
        playerPenalty.Game,
        playerPenalty.Points,
        playerPenalty.Details);

    public async Task<string?> GetGameName(string leagueId, string seasonId, string divisionId, int id, CancellationToken cancellationToken = default)
    {
        var division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);
        if (division == null)
            return null;
        var gameIdx = Array.IndexOf(division.Games, id);
        return gameIdx < 0 ? null : $"G{gameIdx + 1}";
    }

    public async Task<PenaltiesViewModel?> GetPenaltiesVm(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league?.Scoring?.Penalties == null)
            return null;

        var division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);
        if (division == null)
            return null;

        var divisionPenaltiesViewModel = new PenaltiesViewModel();

        foreach (var (gameId, gameName) in division.Games
                     .Select((gameId, i) => (gameId, $"G{i + 1}")))
        {
            if (gameId == null)
                continue;

            var gamePenalties = await _gameService.GetPenaltiesVm(gameId.Value, gameName, league.Scoring.Penalties, cancellationToken);
        }

        return divisionPenaltiesViewModel;
    }
}