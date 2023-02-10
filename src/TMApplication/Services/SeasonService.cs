using TMApplication.Providers;
using TMApplication.ViewModels;
using TMModels;

namespace TMApplication.Services;

public class SeasonService
{
    private readonly IDataProvider _dataProvider;

    public SeasonService(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<SeasonSummaryViewModel> GetSeasonSummaryVm(
        string leagueId, string seasonId, CancellationToken cancellationToken = default)
    {
        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        return new SeasonSummaryViewModel(leagueId, seasonId, season?.Name,
            season?.Divisions ?? Array.Empty<string>());
    }

    public async Task<SeasonChampionViewModel?> GetSeasonChampionVm(
        string leagueId, string seasonId, CancellationToken cancellationToken = default)
    {
        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
            return null;

        var divisionId = season.Divisions.First();
        var division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);
        if (division == null)
            return null;

        if (division.IsFinished)
        {
            var results = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);
            if (results != null)
            {
                var player = results.Players.FirstOrDefault();
                if (player == null)
                    return null;

                return new SeasonChampionViewModel(seasonId, season.Name, player.Player, division.WinnerTitle ?? "The Champion");
            }
        }

        return null;
    }

    public async Task<SeasonDivisionsViewModel?> GetSeasonDivisionsVm(
        string leagueId, string seasonId, CancellationToken cancellationToken = default)
    {
        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
            return null;

        var champions = new List<DivisionChampionViewModel>();
        foreach (var divisionId in season.Divisions)
        {
            var division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);
            if (division is not { IsFinished: true })
                continue;

            var results = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);
            var player = results?.Players.FirstOrDefault();
            if (player == null)
                continue;

            var divisionChampion = new DivisionChampionViewModel(
                divisionId, division.Name, player.Player, division.WinnerTitle);
            champions.Add(divisionChampion);
        }

        return new SeasonDivisionsViewModel(seasonId, season.Name, champions);
    }
}