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
    IReadOnlyCollection<GameSummaryViewModel?> Games);

public record GameSummaryViewModel(
    uint Id,
    string Name,
    double Progress,
    uint Turn,
    bool IsFinished,
    bool IsStalling,
    string? WinnerPlayerName
);

public record GamePlayerSummaryViewModel(
    string GameId,
    string Name,
    string House,
    string? AvatarUri);