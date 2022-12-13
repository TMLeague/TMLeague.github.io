using TMLeague.Http;
using TMLeague.ViewModels;

namespace TMLeague.Services
{
    public class GameService
    {
        private readonly ThroneMasterApi _throneMasterApi;

        public GameService(ThroneMasterApi throneMasterApi)
        {
            _throneMasterApi = throneMasterApi;
        }

        public async Task<GameSummaryViewModel?> GetGameSummaryVm(uint gameId, CancellationToken cancellationToken = default)
        {
            var state = await _throneMasterApi.GetState(gameId, cancellationToken);
            string name = "";
            uint turn = 0;
            double progress = 0;
            return new GameSummaryViewModel(gameId, name, progress, turn, false, false, null);
        }
    }
}
