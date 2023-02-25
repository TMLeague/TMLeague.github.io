namespace TMApplication.ViewModels;

public record SeasonViewModel(
    string Name,
    IReadOnlyList<SeasonDivisionViewModel> Divisions);

public record SeasonDivisionViewModel(
    string Id,
    string Name,
    IReadOnlyList<SeasonPlayerViewModel> Players);

public record SeasonPlayerViewModel(
    string Name,
    decimal Points);