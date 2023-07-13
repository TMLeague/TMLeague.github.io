using TMModels;

namespace TMApplication.ViewModels;

public record PlayerViewModel(
    string Name,
    PlayerLeagueViewModel[] Leagues,
    DateTimeOffset GeneratedTime);

public record PlayerLeagueViewModel(
    string LeagueId,
    string LeagueName,
    PlayerSeasonScoreViewModel[] Seasons);

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