using TMModels;

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

public record SeasonSummaryViewModel(
    string LeagueId,
    string SeasonId,
    string? SeasonName,
    IReadOnlyCollection<string> DivisionIds);

public record DivisionSummaryViewModel(
    string LeagueId,
    string SeasonId,
    string DivisionId,
    string? DivisionName,
    double Progress,
    IReadOnlyCollection<GameSummaryViewModel> Games,
    string? WinnerPlayerName,
    DateTimeOffset GeneratedTime);

public record GameSummaryViewModel(
    uint Id,
    string? Name,
    double Progress,
    uint Turn,
    bool IsFinished,
    bool IsStalling,
    string? WinnerPlayerName,
    DateTimeOffset? GeneratedTime
);

public record GamePlayerSummaryViewModel(
    string GameId,
    string Name,
    House House,
    string? AvatarUri);

public record LeagueChampionsViewModel(
    IReadOnlyList<SeasonChampionViewModel> Champions);

public record SeasonChampionViewModel(
    string SeasonId,
    string SeasonName,
    string PlayerName,
    string Title);