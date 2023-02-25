using TMApplication.Providers;
using TMApplication.ViewModels;

namespace TMApplication.Services;

public class SeasonService
{
    private readonly IDataProvider _dataProvider;

    public SeasonService(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<LeagueSeasonSummaryViewModel> GetSeasonSummaryVm(
        string leagueId, string seasonId, CancellationToken cancellationToken = default)
    {
        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        return new LeagueSeasonSummaryViewModel(leagueId, seasonId, season?.Name,
            season?.Divisions ?? Array.Empty<string>());
    }

    public async Task<LeagueSeasonChampionViewModel?> GetSeasonChampionVm(
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

                return new LeagueSeasonChampionViewModel(seasonId, season.Name, player.Player, division.WinnerTitle);
            }
        }

        return null;
    }

    public async Task<LeagueSeasonViewModel?> GetSeasonDivisionsVm(
        string leagueId, string seasonId, CancellationToken cancellationToken = default)
    {
        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
            return null;

        var champions = new List<LeagueDivisionChampionViewModel>();
        foreach (var divisionId in season.Divisions)
        {
            var division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);
            if (division == null)
                continue;

            if (!division.IsFinished)
            {
                champions.Add(new LeagueDivisionChampionViewModel(divisionId, division.Name, null, null));
                continue;
            }

            var results = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);
            var player = results?.Players.FirstOrDefault();
            if (player == null)
                continue;

            var divisionChampion = new LeagueDivisionChampionViewModel(
                divisionId, division.Name, player.Player, division.WinnerTitle);
            champions.Add(divisionChampion);
        }

        return new LeagueSeasonViewModel(seasonId, season.Name, champions);
    }

    public async Task<SeasonViewModel?> GetSeasonVm(
        string leagueId, string seasonId, CancellationToken cancellationToken = default)
    {
        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
            return null;

        var divisions = new List<SeasonDivisionViewModel>();
        foreach (var divisionId in season.Divisions)
        {
            var division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);
            if (division == null)
                continue;

            var results = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);
            if (results == null)
                continue;

            var players = results.Players.Select(playerResult =>
                new SeasonPlayerViewModel(playerResult.Player, playerResult.TotalPoints)).ToList();
            var divisionVm = new SeasonDivisionViewModel(divisionId, division.Name, players);
            divisions.Add(divisionVm);
        }

        return new SeasonViewModel(season.Name, divisions);
    }
}