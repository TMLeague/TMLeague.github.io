namespace TMApplication.ViewModels;

public record PlayerViewModel(
    string Name,
    PlayerHouseScore[] Houses,
    PlayerGameScore[] Games,
    DateTimeOffset GeneratedTime);

public record PlayerHouseScore
{
}

public record PlayerGameScore
{
}