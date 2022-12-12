namespace TMLeague.ViewModels;

public record HomeViewModel(
    IReadOnlyCollection<HomeLeagueButtonViewModel> Leagues);

public record HomeLeagueButtonViewModel(
    string Id, 
    string Name, 
    string? BackgroundImage);