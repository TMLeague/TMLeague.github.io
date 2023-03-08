using TMModels;

namespace TMApplication.ViewModels;

public record GameViewModel(
    int Id,
    string Name,
    bool IsFinished,
    int Turn,
    HouseScore[] Houses,
    DateTimeOffset GeneratedTime);