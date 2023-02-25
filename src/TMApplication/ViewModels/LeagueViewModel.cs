namespace TMApplication.ViewModels;

public record LeagueViewModel(
    string Id,
    string? Name = null,
    string? Description = null,
    string? Rules = null,
    string? Discord = null,
    IReadOnlyCollection<LeagueSeasonButtonViewModel>? Seasons = null);

public record LeagueSeasonButtonViewModel(
    string Id,
    string Name);

public record LeagueSeasonSummaryViewModel(
    string LeagueId,
    string SeasonId,
    string? SeasonName,
    IReadOnlyCollection<string> DivisionIds);

public record LeagueDivisionSummaryViewModel(
    string LeagueId,
    string SeasonId,
    string DivisionId,
    string? DivisionName,
    double Progress,
    IReadOnlyCollection<LeagueGameSummaryViewModel> Games,
    string? WinnerPlayerName)
{
    public DateTimeOffset? GeneratedTime => Games
        .Max(game => game?.GeneratedTime ?? DateTimeOffset.MinValue);
}

public record LeagueGameSummaryViewModel(
    int Id,
    string? Name,
    double Progress,
    int Turn,
    bool IsFinished,
    bool IsStalling,
    string? WinnerPlayerName,
    DateTimeOffset? GeneratedTime);

public record LeagueChampionsViewModel(
    IReadOnlyList<LeagueSeasonChampionViewModel> Champions);

public record LeagueSeasonChampionViewModel(
    string SeasonId,
    string SeasonName,
    string PlayerName,
    string? Title);

public record LeagueSeasonsViewModel(
    string LeagueId,
    string LeagueName,
    IReadOnlyList<LeagueSeasonViewModel> Seasons);

public record LeagueSeasonViewModel(
    string SeasonId,
    string SeasonName,
    IReadOnlyList<LeagueDivisionChampionViewModel> Divisions);

public record LeagueDivisionChampionViewModel(
    string DivisionId,
    string DivisionName,
    string? PlayerName,
    string? Title);