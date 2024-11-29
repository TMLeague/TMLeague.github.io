using TMModels;

namespace TMApplication.ViewModels;

public record LeagueViewModel(
    string Id,
    string? Name,
    string? Description,
    string? Rules,
    string? Discord,
    LeagueSeasonButtonViewModel? LastSeason)
{
    public LeagueViewModel(string id) :
        this(id, null, null,
            null, null, null)
    { }
};

public record LeagueSeasonButtonViewModel(
    string Id,
    string Name,
    DateTimeOffset? GeneratedTime);

public record LeagueSeasonSummaryViewModel(
    string LeagueId,
    string SeasonId,
    string? SeasonName,
    IReadOnlyCollection<string> DivisionIds,
    DateTimeOffset? PlannedStart,
    DateTimeOffset? PlannedEnd,
    double Progress);

public record LeagueDivisionSummaryViewModel(
    string LeagueId,
    string SeasonId,
    string DivisionId,
    string? DivisionName,
    double Progress,
    IReadOnlyCollection<LeagueGameSummaryViewModel> Games,
    string? WinnerPlayerName,
    string? JudgeTitle,
    string? Judge)
{
    public DateTimeOffset? GeneratedTime =>
        Games.Count == 0 ?
            DateTimeOffset.UtcNow :
            Games.Max(game => game?.GeneratedTime ?? DateTimeOffset.MinValue);
}

public record LeagueGameSummaryViewModel(
    int? Id,
    string? Name,
    double Progress,
    int Turn,
    bool IsFinished,
    bool IsStalling,
    string? WinnerPlayerName,
    DateTimeOffset? GeneratedTime,
    DateTimeOffset? LastActionTime)
{
    public TimeSpan TimeSinceLastAction => LastActionTime.HasValue ? DateTimeOffset.UtcNow - LastActionTime.Value : TimeSpan.Zero;
}
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