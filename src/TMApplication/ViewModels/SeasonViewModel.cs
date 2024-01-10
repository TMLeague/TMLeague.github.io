using TMModels;

namespace TMApplication.ViewModels;

public record SeasonViewModel(
    string LeagueName,
    string SeasonName,
    IReadOnlyList<SeasonDivisionViewModel> Divisions,
    Navigation SeasonNavigation);

public record SeasonDivisionViewModel(
    string Id,
    string Name,
    IReadOnlyList<SeasonPlayerViewModel> Players);

public record SeasonPlayerViewModel(
    string Name,
    double Points,
    bool IsPromoted,
    bool IsRelegated);