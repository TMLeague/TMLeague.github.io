namespace TMLeague.ViewModels
{
    public record LeaguesViewModel(IReadOnlyCollection<LeagueViewModel> Leagues);

    public record LeagueViewModel(string Id, string Name, string? BackgroundImage);
}
