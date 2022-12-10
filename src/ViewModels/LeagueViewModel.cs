namespace TMLeague.ViewModels
{
    public record LeagueViewModel(
        string Id,
        string? Name = null,
        LeagueSeasonViewModel? CurrentSeason = null,
        IReadOnlyCollection<LeagueSeasonButtonViewModel>? Seasons = null);

    public record LeagueSeasonViewModel();

    public record LeagueSeasonButtonViewModel(
        string Id,
        string Name);
}
