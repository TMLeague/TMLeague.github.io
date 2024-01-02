using System.Diagnostics;
using TMApplication.Extensions;
using TMApplication.Providers;
using TMApplication.ViewModels;
using TMModels;

namespace TMApplication.Services;

public class LeagueService
{
    private readonly IDataProvider _dataProvider;
    private readonly SeasonService _seasonService;
    private readonly DraftService _draftService;
    private readonly PlayerStatsService _playerStatsService;

    public LeagueService(IDataProvider dataProvider, SeasonService seasonService, DraftService draftService, PlayerStatsService playerStatsService)
    {
        _dataProvider = dataProvider;
        _seasonService = seasonService;
        _draftService = draftService;
        _playerStatsService = playerStatsService;
    }

    public async Task<LeagueViewModel> GetLeagueVm(string leagueId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return new LeagueViewModel(leagueId);

        var seasonButton = await GetSeasonButtonViewModel(leagueId, league, cancellationToken);
        return new LeagueViewModel(leagueId, league.Name, league.Description, league.Rules, league.Discord, seasonButton);
    }

    private async Task<LeagueSeasonButtonViewModel?> GetSeasonButtonViewModel(string leagueId, League league, CancellationToken cancellationToken)
    {
        var seasonId = league.AllSeasons.LastOrDefault();
        if (seasonId == null)
            return null;

        var season = await _dataProvider.GetSeason(leagueId, seasonId, cancellationToken);
        if (season == null)
            return null;

        var results = new List<Results>();
        foreach (var divisionId in season.Divisions)
        {
            var result = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);
            if (result != null)
                results.Add(result);
        }

        var generatedTime = results.Count > 0 ?
            results.Max(result => result.GeneratedTime) :
            DateTimeOffset.UtcNow;

        return new LeagueSeasonButtonViewModel(seasonId, season.Name, generatedTime);
    }

    public async Task<LeagueSeasonChampionViewModel?> GetLeagueChampionVm(string leagueId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return null;

        if (league.Seasons.Length < 1)
            return null;

        var champion = await _seasonService.GetSeasonChampionVm(leagueId, league.Seasons.Last(), cancellationToken);
        if (champion != null)
            return champion;

        if (league.Seasons.Length < 2)
            return null;

        return await _seasonService.GetSeasonChampionVm(leagueId, league.Seasons[^2], cancellationToken);
    }

    public async Task<DivisionSetupViewModel?> GetDivisionSetupVm(string leagueId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return null;

        var nextMainSeason = league.Seasons.Length == 0 ?
            "1" :
            league.Seasons
                .Max(season =>
                    int.TryParse(season[1..], out var seasonNumber) ?
                        seasonNumber + 1 :
                        1)
                .ToString();

        return new DivisionSetupViewModel(
            league.Name,
            league.InitialMessage?.Subject ?? string.Empty,
            league.InitialMessage?.Body == null ?
                string.Empty :
                string.Join(Environment.NewLine, league.InitialMessage.Body),
            nextMainSeason);
    }

    public async Task<DivisionDraft?> GetBestDraft(DivisionForm divisionForm, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        DivisionDraft? best;
        do
        {
            best = await GetDraft(divisionForm, cancellationToken);
        } while (best == null && sw.Elapsed < divisionForm.RandomOptions.Timeout);

        if (best == null)
            return null;

        var bestScore = best.GetScore(divisionForm.RandomOptions.Weights);
        while (divisionForm.RandomOptions.UseRandomDraft && sw.Elapsed < divisionForm.RandomOptions.Timeout)
        {
            var draft = await GetDraft(divisionForm, cancellationToken);
            var draftScore = draft?.GetScore(divisionForm.RandomOptions.Weights) ?? int.MinValue;
            if (draft == null || draftScore < bestScore)
                continue;

            best = draft;
            bestScore = draftScore;
        }

        return best;
    }

    public async Task<DivisionDraft?> GetDraft(DivisionForm divisionForm, CancellationToken cancellationToken = default)
    {
        if (divisionForm.League == null)
            return null;

        var league = await _dataProvider.GetLeague(divisionForm.League, cancellationToken);
        if (league == null)
            return null;

        var playersLength = divisionForm.PlayerNames.Count;
        if (playersLength < 3)
            return null;

        var drafts = divisionForm.RandomOptions.UseRandomDraft ?
            Array.Empty<Draft>() :
            await _dataProvider.GetDrafts(playersLength, cancellationToken);

        var draft = GetDraftInternal(drafts, playersLength, out var isRandom);
        if (draft == null)
            return null;

        var draftTable = draft.Table.Select(housesTemplate =>
            housesTemplate.Select(HouseParser.Parse).ToArray()).ToArray();
        var players = divisionForm.PlayerNames.OrderBy(p => p).ToArray();
        var stats = _playerStatsService.GetStats(draftTable, players);

        var messageSubject = divisionForm.MessageSubject?.FillParameters(divisionForm) ?? string.Empty;
        var messageBody = divisionForm.MessageBody?.FillParameters(divisionForm) ?? string.Empty;
        var playerDrafts = players
            .Zip(draftTable, stats)
            .Select(playerKv => new PlayerDraft(
                playerKv.First,
                playerKv.Second,
                messageSubject,
                messageBody.FillParameters(GetPlayerHouseGames(playerKv.Second)), playerKv.Third)).ToList();

        var divisionDraft = new DivisionDraft(playerDrafts, isRandom);

        return divisionDraft;
    }

    private Draft? GetDraftInternal(Draft[] drafts, int playersLength, out bool isRandom)
    {
        if (drafts.Length > 0)
        {
            isRandom = false;
            return _draftService.GetDraft(drafts[Random.Shared.Next(drafts.Length)]);
        }

        isRandom = true;
        return _draftService.GetDraft(playersLength, 6);
    }

    public async Task<LeagueSeasonsViewModel?> GetLeagueSeasonsVm(string leagueId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return null;

        var divisions = new List<LeagueSeasonViewModel>();
        foreach (var seasonId in league.AllSeasons.Reverse())
        {
            var seasonDivisionsVm = await _seasonService.GetSeasonDivisionsVm(leagueId, seasonId, cancellationToken);
            if (seasonDivisionsVm is { Divisions.Count: > 0 })
                divisions.Add(seasonDivisionsVm);
        }

        return new LeagueSeasonsViewModel(leagueId, league.Name, divisions);
    }

    public async Task<LeagueChampionsViewModel?> GetLeagueChampionsVm(string leagueId, CancellationToken cancellationToken = default)
    {
        var league = await _dataProvider.GetLeague(leagueId, cancellationToken);
        if (league == null)
            return null;

        var champions = new List<LeagueSeasonChampionViewModel>();
        foreach (var seasonId in league.Seasons.Reverse())
        {
            var champion = await _seasonService.GetSeasonChampionVm(leagueId, seasonId, cancellationToken);
            if (champion != null)
                champions.Add(champion);
        }

        return new LeagueChampionsViewModel(champions);
    }

    private static PlayerHouseGames GetPlayerHouseGames(House[] houses) => new(
        Array.IndexOf(houses, House.Baratheon) + 1,
        Array.IndexOf(houses, House.Lannister) + 1,
        Array.IndexOf(houses, House.Stark) + 1,
        Array.IndexOf(houses, House.Tyrell) + 1,
        Array.IndexOf(houses, House.Greyjoy) + 1,
        Array.IndexOf(houses, House.Martell) + 1,
        Array.IndexOf(houses, House.Arryn) + 1);
}