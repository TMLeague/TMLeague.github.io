namespace TMLeague.ViewModels
{
    public record LeagueViewModel(
        string Id,
        string? Name = null,
        string? Description = null,
        string? Rules = null,
        string? Discord = null,
        LeagueSeasonViewModel? CurrentSeason = null,
        IReadOnlyCollection<LeagueSeasonButtonViewModel>? Seasons = null);

    public record LeagueSeasonViewModel();

    public record LeagueSeasonButtonViewModel(
        string Id,
        string Name);
}
