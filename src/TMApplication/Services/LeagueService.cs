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
    private readonly IPasswordGenerator _generator;

    public LeagueService(IDataProvider dataProvider, SeasonService seasonService, DraftService draftService, PlayerStatsService playerStatsService, IPasswordGenerator generator)
    {
        _dataProvider = dataProvider;
        _seasonService = seasonService;
        _draftService = draftService;
        _playerStatsService = playerStatsService;
        _generator = generator;
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

        var nextMainSeason = "1";
        if (league.Seasons.Length > 0)
        {
            var lastSeasonId = league.Seasons.Last();
            var lastSeason = await _dataProvider.GetSeason(leagueId, lastSeasonId, cancellationToken);
            if (await IsNextSeason(leagueId, lastSeasonId, lastSeason, cancellationToken))
                nextMainSeason = lastSeasonId[1..];
            else
                nextMainSeason = league.Seasons
                .Max(season =>
                    int.TryParse(season[1..], out var seasonNumber) ? seasonNumber + 1 : 1)
                .ToString();
        }

        return new DivisionSetupViewModel(
            league.Name,
            league.InitialMessage?.Subject ?? string.Empty,
            league.InitialMessage?.Body == null ?
                string.Empty :
                string.Join(Environment.NewLine, league.InitialMessage.Body),
            nextMainSeason);
    }

    private async Task<bool> IsNextSeason(string leagueId, string seasonId, Season? lastSeason, CancellationToken cancellationToken)
    {
        if (lastSeason == null)
            return true;

        foreach (var divisionId in lastSeason.Divisions)
        {
            var division = await _dataProvider.GetResults(leagueId, seasonId, divisionId, cancellationToken);
            if (division == null || division.IsFinished == false)
                return true;
        }

        return false;
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
            housesTemplate.Select(Houses.Parse).ToArray()).ToArray();
        var players = divisionForm.PlayerNames.OrderBy(p => p).ToArray();
        var stats = _playerStatsService.GetStats(draftTable, players);
        var passwords = (await _generator.Get(league.InitialMessage?.PasswordLength ?? 6, playersLength, cancellationToken)).ToArray();

        var messageSubject = divisionForm.MessageSubject?.FillParameters(divisionForm) ?? string.Empty;
        var messageBody = divisionForm.MessageBody?.FillParameters(divisionForm) ?? string.Empty;
        var playerDrafts = players
            .Zip(draftTable, stats)
            .Select(playerKv => new PlayerDraft(
                playerKv.First,
                playerKv.Second,
                messageSubject,
                messageBody.FillParameters(GetPlayerDraftParameters(playerKv.First, playerKv.Second, passwords)),
                playerKv.Third)).ToList();

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

    private static PlayerDraftParameters GetPlayerDraftParameters(string name, House[] houses, IReadOnlyList<string> passwords)
    {
        var baratheonIdx = Array.IndexOf(houses, House.Baratheon);
        var lannisterIdx = Array.IndexOf(houses, House.Lannister);
        var starkIdx = Array.IndexOf(houses, House.Stark);
        var tyrellIdx = Array.IndexOf(houses, House.Tyrell);
        var greyjoyIdx = Array.IndexOf(houses, House.Greyjoy);
        var martellIdx = Array.IndexOf(houses, House.Martell);
        var arrynIdx = Array.IndexOf(houses, House.Arryn);
        return new PlayerDraftParameters(
            name,
            baratheonIdx + 1, baratheonIdx >= 0 ? passwords[baratheonIdx] : "",
            lannisterIdx + 1, lannisterIdx >= 0 ? passwords[lannisterIdx] : "",
            starkIdx + 1, starkIdx >= 0 ? passwords[starkIdx] : "",
            tyrellIdx + 1, tyrellIdx >= 0 ? passwords[tyrellIdx] : "",
            greyjoyIdx + 1, greyjoyIdx >= 0 ? passwords[greyjoyIdx] : "",
            martellIdx + 1, martellIdx >= 0 ? passwords[martellIdx] : "",
            arrynIdx + 1, arrynIdx >= 0 ? passwords[arrynIdx] : "");
    }
}