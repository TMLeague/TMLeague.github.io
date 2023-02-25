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
    double TotalPoints = 0,
    int Wins = 0,
    int Cla = 0,
    int Supplies = 0,
    int PowerTokens = 0,
    double MinutesPerMove = 0,
    int Moves = 0,
    PlayerHouseViewModel[]? Houses = null,
    double TotalPenaltyPoints = 0,
    PlayerPenaltyViewModel[]? Penalties = null)
{
    public PlayerHouseViewModel GetHouse(House house) =>
        Houses?.FirstOrDefault(h => h.House == house) ??
        new PlayerHouseViewModel(null, house);
}

public record PlayerHouseViewModel(
    int? Game,
    House House,
    bool IsWinner = false,
    double Points = 0,
    int BattlePenalty = 0,
    int Strongholds = 0,
    int Castles = 0,
    int Cla = 0,
    int Supplies = 0,
    int PowerTokens = 0,
    double MinutesPerMove = 0,
    int Moves = 0);

public record PlayerPenaltyViewModel(
    int? Game,
    double Points,
    string Details);