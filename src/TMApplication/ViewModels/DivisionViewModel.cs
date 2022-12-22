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
    DivisionPlayerViewModel[] Players);

public record DivisionPlayerViewModel(
    string Name,
    short TotalPoints = 0,
    ushort Wins = 0,
    ushort Cla = 0,
    ushort Supplies = 0,
    ushort PowerTokens = 0,
    double MinutesPerMove = 0,
    ushort Moves = 0,
    PlayerHouseViewModel[]? Houses = null,
    ushort TotalPenaltyPoints = 0,
    PlayerPenaltyViewModel[]? Penalties = null)
{
    public PlayerHouseViewModel GetHouse(House house) =>
        Houses?.FirstOrDefault(h => h.House == house) ??
        new PlayerHouseViewModel(null, house);
}

public record PlayerHouseViewModel(
    uint? Game,
    House House,
    bool IsWinner = false,
    ushort Points = 0,
    ushort BattlePenalty = 0,
    ushort Strongholds = 0,
    ushort Castles = 0,
    ushort Cla = 0,
    ushort Supplies = 0,
    ushort PowerTokens = 0,
    double MinutesPerMove = 0,
    ushort Moves = 0);

public record PlayerPenaltyViewModel(
    uint? Game,
    ushort Points,
    string Details);