using TMModels;

namespace TMApplication.ViewModels;

public record DivisionViewModel(
    string LeagueName,
    string SeasonName,
    string DivisionName,
    string JudgeTitle,
    string Judge,
    bool IsFinished,
    string? WinnerTitle,
    DivisionPlayerViewModel[] Players,
    DivisionGameViewModel?[] Games,
    Tiebreaker[] Tiebreakers,
    NotificationMessage[] Messages,
    DateTimeOffset? GeneratedTime,
    Navigation SeasonNavigation,
    Navigation DivisionNavigation,
    Replacement[] Replacements,
    int? Promotions,
    int? Relegations);

public record DivisionPlayerViewModel(
    string Name,
    double TotalPoints,
    int Wins,
    int Cla,
    int Supplies,
    int PowerTokens,
    double MinutesPerMove,
    int Moves,
    PlayerHouseViewModel[] Houses,
    double TotalPenaltyPoints,
    PlayerPenaltyViewModel[] Penalties,
    Stats Stats,
    bool IsPromoted,
    bool IsRelegated)
{
    public DivisionPlayerViewModel(string name) :
        this(name, 0, 0, 0, 0, 0, 0, 0,
            Array.Empty<PlayerHouseViewModel>(), 0,
            Array.Empty<PlayerPenaltyViewModel>(),
            new Stats(), false, false)
    { }

    public PlayerHouseViewModel GetHouse(House house) =>
        Houses?.FirstOrDefault(h => h.House == house) ??
        new PlayerHouseViewModel(null, house);
}

public record DivisionGameViewModel(
    int Id,
    string? Name,
    int Turn,
    double Progress,
    bool IsFinished,
    bool IsStalling,
    bool IsCreatedManually,
    DateTimeOffset? LastActionTime);

public record PlayerHouseViewModel(
    int? Game,
    House House,
    bool IsWinner,
    double Points,
    int BattlePenalty,
    int Strongholds,
    int Castles,
    int Cla,
    int Supplies,
    int PowerTokens,
    double MinutesPerMove,
    int Moves,
    Stats Stats)
{
    public PlayerHouseViewModel(int? game, House house) :
        this(game, house, false, 0, 0, 0, 0, 0, 0, 0, 0, 0, new Stats())
    { }
}

public record PlayerPenaltyViewModel(
    int? Game,
    double Points,
    string Details);

public record NotificationMessage(
    NotificationLevel Level,
    string Message,
    bool IsStallingOrFinished);

public enum NotificationLevel
{
    Info, Warning, Critical
}