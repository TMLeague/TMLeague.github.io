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

    public async Task<LeagueSeasonSummaryViewModel> GetSeasonSummaryVm(
        string leagueId, string seasonId, CancellationToken cancellationToken = default)
    {
        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        var progress = 100 * Math.Min(
            Math.Max((DateTimeOffset.UtcNow - season?.StartDate) / (season?.EndDate - season?.StartDate) ?? 0, 0), 1);

        return new LeagueSeasonSummaryViewModel(leagueId, seasonId, season?.Name,
            season?.Divisions ?? Array.Empty<string>(), season?.StartDate, season?.EndDate, progress);
    }

    public async Task<LeagueSeasonChampionViewModel?> GetSeasonChampionVm(
        string leagueId, string seasonId, CancellationToken cancellationToken = default)
    {
        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
            return null;

        var divisionId = season.Divisions.First();
        var division = await _dataProvider.GetDivision(leagueId, seasonId, divisionId, cancellationToken);
        if (division is not { IsFinished: true })
            return null;

        var results = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);
        if (results == null)
            return null;

        var player = results.Players.FirstOrDefault();
        return player == null ? null :
            new LeagueSeasonChampionViewModel(seasonId, season.Name, player.Player, division.WinnerTitle);
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
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);

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
            var players = results == null ?
                division.Players.Select(name => new SeasonPlayerViewModel(name, 0, false, false)).ToList() :
                results.Players.Select(playerResult => new SeasonPlayerViewModel(
                    playerResult.Player, playerResult.TotalPoints, playerResult.IsPromoted, playerResult.IsRelegated)).ToList();

            var divisionVm = new SeasonDivisionViewModel(divisionId, division.Name, players);
            divisions.Add(divisionVm);
        }

        return new SeasonViewModel(league?.Name ?? $"League {leagueId}", season.Name, divisions, league?.GetSeasonNavigation(seasonId) ?? new Navigation());
    }
}