using TMModels;

namespace TMApplication.ViewModels;

public record GameViewModel(
    int Id,
    string Name,
    bool IsFinished,
    bool IsStalling,
    int Turn,
    HouseScore[] Houses,
    DateTimeOffset GeneratedTime);