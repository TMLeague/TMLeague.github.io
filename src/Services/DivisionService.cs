using TMLeague.Http;
using TMLeague.ViewModels;

namespace TMLeague.Services
{
    public class DivisionService
    {
        private readonly LocalApi _localApi;
        private readonly GameService _gameService;

        public DivisionService(LocalApi localApi, GameService gameService)
        {
            _localApi = localApi;
            _gameService = gameService;
        }

        public async Task<DivisionSummaryViewModel?> GetDivisionSummaryVm(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken = default)
        {
            var division = await _localApi.GetDivision(leagueId, seasonId, divisionId, cancellationToken);

            var games = new List<GameSummaryViewModel?>();

            if (division == null)
                return new DivisionSummaryViewModel(leagueId, seasonId, divisionId, division?.Name, 0, games);

            var progress = 0.0;
            foreach (var gameId in division.Games)
            {
                var game = await _gameService.GetGameSummaryVm(gameId, cancellationToken);
                if (game == null)
                    continue;

                games.Add(game);
                progress += game.Progress;
            }

            progress /= games.Count;

            return new DivisionSummaryViewModel(leagueId, seasonId, divisionId, division?.Name, progress, games);
        }
    }
}
