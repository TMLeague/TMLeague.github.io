namespace TMApplication.ViewModels;

public enum ViewColumn
{
    Default,
    House,
    Player,
    Points,
    Wins,
    Penalties,
    Cla,
    Supply,
    PT,
    Baratheon,
    Lannister,
    Stark,
    Tyrell,
    Greyjoy,
    Martell,
    Position,
    MPM,
    Moves,
    Battles,
    Kills,
    Casualties,
    PowerTokensGathered,
    PowerTokensSpent,
    AllSeasons
}

public static class SummaryColumns
{
    public static IEnumerable<PlayerScoreViewModel> Sort(this IEnumerable<PlayerScoreViewModel> players, ViewColumn column, bool sortAscending, ScoreType scoreType) =>
        column switch
        {
            ViewColumn.Player => players.OrderBy(player => player.PlayerName).SortWithDirection(sortAscending),
            ViewColumn.Points => players.OrderBy(player => player.Scores[scoreType].TotalPoints).SortWithDirection(sortAscending),
            ViewColumn.Wins => players.OrderBy(player => player.Scores[scoreType].Wins).SortWithDirection(sortAscending),
            ViewColumn.Penalties => players.OrderBy(player => player.Scores[scoreType].PenaltiesPoints).SortWithDirection(sortAscending),
            ViewColumn.Cla => players.OrderBy(player => player.Scores[scoreType].Cla).SortWithDirection(sortAscending),
            ViewColumn.Supply => players.OrderBy(player => player.Scores[scoreType].Supplies).SortWithDirection(sortAscending),
            ViewColumn.PT => players.OrderBy(player => player.Scores[scoreType].PowerTokens).SortWithDirection(sortAscending),
            ViewColumn.Baratheon => players.OrderBy(player => player.Scores[scoreType].Baratheon).SortWithDirection(sortAscending),
            ViewColumn.Lannister => players.OrderBy(player => player.Scores[scoreType].Lannister).SortWithDirection(sortAscending),
            ViewColumn.Stark => players.OrderBy(player => player.Scores[scoreType].Stark).SortWithDirection(sortAscending),
            ViewColumn.Tyrell => players.OrderBy(player => player.Scores[scoreType].Tyrell).SortWithDirection(sortAscending),
            ViewColumn.Greyjoy => players.OrderBy(player => player.Scores[scoreType].Greyjoy).SortWithDirection(sortAscending),
            ViewColumn.Martell => players.OrderBy(player => player.Scores[scoreType].Martell).SortWithDirection(sortAscending),
            ViewColumn.Position => players.OrderBy(player => player.Scores[scoreType].Position).SortWithDirection(sortAscending),
            ViewColumn.MPM => players.OrderBy(player => player.Scores[scoreType].MinutesPerMove).SortWithDirection(sortAscending),
            ViewColumn.Battles => players.OrderBy(player => player.Scores[scoreType].Stats.Battles.Total).SortWithDirection(sortAscending),
            ViewColumn.Kills => players.OrderBy(player => player.Scores[scoreType].Stats.Kills.Total).SortWithDirection(sortAscending),
            ViewColumn.Casualties => players.OrderBy(player => player.Scores[scoreType].Stats.Casualties.Total).SortWithDirection(sortAscending),
            ViewColumn.PowerTokensGathered => players.OrderBy(player => player.Scores[scoreType].Stats.PowerTokens.Total).SortWithDirection(sortAscending),
            ViewColumn.PowerTokensSpent => players.OrderBy(player => player.Scores[scoreType].Stats.Bids.Total).SortWithDirection(sortAscending),
            ViewColumn.AllSeasons => players.OrderBy(player => player.Seasons).SortWithDirection(sortAscending),
            _ => players
        };

    public static IEnumerable<HouseScoreViewModel> Sort(this IEnumerable<HouseScoreViewModel> houses, ViewColumn column, bool sortAscending, ScoreType scoreType) =>
        column switch
        {
            ViewColumn.Player => houses.OrderBy(house => house.House).SortWithDirection(sortAscending),
            ViewColumn.Points => houses.OrderBy(house => house.Scores[scoreType].Points).SortWithDirection(sortAscending),
            ViewColumn.Wins => houses.OrderBy(house => house.Scores[scoreType].Wins).SortWithDirection(sortAscending),
            ViewColumn.Cla => houses.OrderBy(house => house.Scores[scoreType].Cla).SortWithDirection(sortAscending),
            ViewColumn.Supply => houses.OrderBy(house => house.Scores[scoreType].Supplies).SortWithDirection(sortAscending),
            ViewColumn.PT => houses.OrderBy(house => house.Scores[scoreType].PowerTokens).SortWithDirection(sortAscending),
            ViewColumn.Moves => houses.OrderBy(player => player.Scores[scoreType].Moves).SortWithDirection(sortAscending),
            ViewColumn.Battles => houses.OrderBy(house => house.Scores[scoreType].Stats.Battles.Total).SortWithDirection(sortAscending),
            ViewColumn.Kills => houses.OrderBy(house => house.Scores[scoreType].Stats.Kills.Total).SortWithDirection(sortAscending),
            ViewColumn.Casualties => houses.OrderBy(house => house.Scores[scoreType].Stats.Casualties.Total).SortWithDirection(sortAscending),
            ViewColumn.PowerTokensGathered => houses.OrderBy(house => house.Scores[scoreType].Stats.PowerTokens.Total).SortWithDirection(sortAscending),
            ViewColumn.PowerTokensSpent => houses.OrderBy(house => house.Scores[scoreType].Stats.Bids.Total).SortWithDirection(sortAscending),
            _ => houses
        };

    private static IEnumerable<PlayerScoreViewModel> SortWithDirection(this IOrderedEnumerable<PlayerScoreViewModel> orderBy, bool sortAscending) =>
        sortAscending ? orderBy : orderBy.Reverse();

    private static IEnumerable<HouseScoreViewModel> SortWithDirection(this IOrderedEnumerable<HouseScoreViewModel> orderBy, bool sortAscending) =>
        sortAscending ? orderBy : orderBy.Reverse();

    public static bool GetSortAscendingDefault(this ViewColumn column) => column switch
    {
        ViewColumn.House => true,
        ViewColumn.Player => true,
        ViewColumn.Points => false,
        ViewColumn.Wins => false,
        ViewColumn.Penalties => true,
        ViewColumn.Cla => false,
        ViewColumn.Supply => false,
        ViewColumn.PT => false,
        ViewColumn.Baratheon => false,
        ViewColumn.Lannister => false,
        ViewColumn.Stark => false,
        ViewColumn.Tyrell => false,
        ViewColumn.Greyjoy => false,
        ViewColumn.Martell => false,
        ViewColumn.Position => true,
        ViewColumn.MPM => true,
        ViewColumn.Moves => false,
        ViewColumn.Battles => false,
        ViewColumn.Kills => false,
        ViewColumn.Casualties => true,
        ViewColumn.PowerTokensGathered => false,
        ViewColumn.PowerTokensSpent => false,
        ViewColumn.AllSeasons => false,
        _ => false
    };
}