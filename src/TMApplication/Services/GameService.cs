using TMApplication.Providers;
using TMApplication.ViewModels;

namespace TMApplication.Services;

public class GameService
{
    private readonly IDataProvider _dataProvider;

    public GameService(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<GameSummaryViewModel> GetGameSummaryVm(uint gameId, CancellationToken cancellationToken = default)
    {
        var game = await _dataProvider.GetGame(gameId, cancellationToken);
        if (game == null)
            return new GameSummaryViewModel(gameId, null, 0, 0, false, false, null, null);

        var progress = game.IsFinished ?
            100 :
            game.IsStalling ?
                97 :
                100 * (double)game.Turn / 11;
        return new GameSummaryViewModel(gameId, game.Name, progress, game.Turn, game.IsFinished, game.IsStalling, game.Winner, game.ModifiedDate);
    }
}