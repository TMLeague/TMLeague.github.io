using System.Runtime.CompilerServices;
using TMApplication.Extensions;
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

        var results = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);
        var winnerPlayerName = results?.IsFinished == true ? results?.Players.First().Player : null;

        return new LeagueDivisionSummaryViewModel(leagueId, seasonId, divisionId, division?.Name, progress, games, winnerPlayerName);
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

        var messages = await GetMessages(leagueId, seasonId, divisionId, division, cancellationToken);

        var games = await GetDivisionGames(division, cancellationToken).ToArrayAsync(cancellationToken);
        return new DivisionViewModel(league.Name, season.Name, division.Name, league.JudgeTitle ?? "Judge", division.Judge ?? string.Empty, results?.IsFinished ?? false, division.WinnerTitle,
            (results?.Players.Select(GetPlayerVm) ??
             division.Players.Select(s => new DivisionPlayerViewModel(s))).ToArray(),
            games,
            league.Scoring?.Tiebreakers ?? Tiebreakers.Default, messages,
            results?.GeneratedTime,
            league.GetSeasonNavigation(seasonId), season.GetDivisionNavigation(divisionId),
            division.Replacements ?? Array.Empty<Replacement>(), division.Promotions, division.Relegations);
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
            if (game == null)
                continue;

            foreach (var houseScore in game.Houses)
            {
                if (houseScore.MinutesPerMove >= 500)
                    messages.Add(new NotificationMessage(
                        NotificationLevel.Critical,
                        $"{houseScore.PlayerHouseName} in <a href=\"{RouteProvider.GetGameRoute(leagueId, game.Id)}\" class=\"text-inherit\">G{gameIdx + 1}</a> has {Math.Round(houseScore.MinutesPerMove)} mpm.",
                        game.IsStalling || game.IsFinished));
                else if (houseScore.MinutesPerMove >= 300)
                    messages.Add(new NotificationMessage(
                        NotificationLevel.Warning,
                        $"{houseScore.PlayerHouseName} in <a href=\"{RouteProvider.GetGameRoute(leagueId, game.Id)}\" class=\"text-inherit\">G{gameIdx + 1}</a> has {Math.Round(houseScore.MinutesPerMove)} mpm.",
                        game.IsStalling || game.IsFinished));
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

    public async Task<DivisionConfigurationForm?> GetDivisionConfigurationForm(string leagueId, string? seasonId, string? divisionId, bool isFinished, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return null;

        if (string.IsNullOrEmpty(seasonId) || string.IsNullOrEmpty(divisionId))
        {
            return new DivisionConfigurationForm(
                string.Empty,
                string.Empty,
                new List<DivisionConfigurationFormPlayer>(),
                new List<DivisionConfigurationFormGame>(),
                new List<DivisionConfigurationFormPenalty>(),
                new List<DivisionConfigurationFormReplacement>(),
                false,
                string.Empty,
                null,
                null);
        }

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

        var players = division.Players
            .Select((player, i) =>
                new DivisionConfigurationFormPlayer(i + 1, GetGames(player, division.Games, results), player))
            .ToList();

        if (results != null)
        {
            var playerGames = players.Max(player => player.Games.Count);
            var incompletePlayers = players.Where(player => player.Games.Count < playerGames).ToArray();
            var games = new Dictionary<DivisionConfigurationFormGame, int>();
            foreach (var player in players)
                foreach (var game in player.Games.Where(game => game.TmId != null))
                {
                    games.TryGetValue(game, out var playersCount);
                    games[game] = playersCount + 1;
                }

            var gameHouses = results.Players
                .SelectMany(result => result.Houses.Select(house => house.House))
                .Distinct().Count();
            var incompleteGames = games
                .Where(pair => pair.Value < gameHouses)
                .Select(pair => pair.Key)
                .OrderBy(game => game.Idx)
                .ToArray();

            foreach (var player in incompletePlayers)
                foreach (var game in incompleteGames)
                    if (!player.Games.Contains(game))
                        player.Games.Add(game);
        }
        var leagueDivision = league.MainDivisions.FirstOrDefault(d => d.Id == divisionId);
        return new DivisionConfigurationForm(
        division.Name,
        division.Judge,
            players,
            division.Games.Select((game, i) => new DivisionConfigurationFormGame(i + 1, game)).ToList(),
            division.Penalties?.Select((penalty, i) => new DivisionConfigurationFormPenalty(i + 1, penalty.Player,
                penalty.Game, penalty.Points, penalty.Details, penalty.Disqualification)).ToList(),
            division.Replacements?.Select((replacement, i) => new DivisionConfigurationFormReplacement(i + 1,
                replacement.From, replacement.To, replacement.Game)).ToList(),
            division.IsFinished || isFinished,
            division.WinnerTitle,
            division.Promotions ?? leagueDivision?.Promotions,
            division.Relegations ?? leagueDivision?.Relegations);
    }

    private static List<DivisionConfigurationFormGame> GetGames(string player, int?[] games, Results? results) =>
        results?.Players
            .FirstOrDefault(result => result.Player == player)?.Houses
            .Select(result => new DivisionConfigurationFormGame(games.IndexOf(result.Game) + 1, result.Game))
            .ToList() ?? new List<DivisionConfigurationFormGame>();

    public async Task<TotalInteractions?> GetDivisionInteractions(string leagueId, string seasonId, string divisionId,
        CancellationToken cancellationToken = default) =>
        await _dataProvider.GetDivisionInteractions(leagueId, seasonId, divisionId, cancellationToken);
}