namespace TMModels;

public record Results(
    PlayerResult[] Players,
    DateTimeOffset GeneratedTime,
    bool IsCreatedManually = false);

public record PlayerResult(
    string Player,
    double TotalPoints,
    int Wins,
    int Cla,
    int Supplies,
    int PowerTokens,
    double MinutesPerMove,
    int Moves,
    HouseResult[] Houses,
    double PenaltiesPoints,
    PlayerPenalty[] PenaltiesDetails,
    Stats? Stats)
{
    public Stats Stats { get; } = Stats ?? new Stats();
}

public record HouseResult(
    int Game,
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
    Stats? Stats)
{
    public Stats Stats { get; } = Stats ?? new Stats();
}

public record PlayerPenalty(
    int? Game,
    double Points,
    string Details);