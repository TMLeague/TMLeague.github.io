using TMApplication.Providers;
using TMApplication.ViewModels;

namespace TMApplication.Services
{
    public class GameService
    {
        private readonly IDataProvider _dataProvider;

        public GameService(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public async Task<GameSummaryViewModel?> GetGameSummaryVm(uint gameId, CancellationToken cancellationToken = default)
        {
            var state = await _dataProvider.GetState(gameId, cancellationToken);
            string name = "";
            uint turn = 0;
            double progress = 0;
            return new GameSummaryViewModel(gameId, name, progress, turn, false, false, null);
        }
    }
}
