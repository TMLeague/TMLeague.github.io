using TMModels;

namespace TMApplication.ViewModels;

public record PlayerLeagueViewModel(
    string Name,
    PlayerSeasonScoreViewModel[] Seasons,
    DateTimeOffset GeneratedTime);

public record PlayerSeasonScoreViewModel(
    string SeasonId,
    string DivisionId,
    double TotalPoints,
    int Wins,
    int Cla,
    int Supplies,
    int PowerTokens,
    double MinutesPerMove,
    int Moves,
    PlayerHouseViewModel[] Houses,
    double PenaltiesPoints,
    Stats? Stats)
{
    public PlayerHouseViewModel GetHouse(House house) =>
        Houses?.FirstOrDefault(h => h.House == house) ??
        new PlayerHouseViewModel(null, house);
}