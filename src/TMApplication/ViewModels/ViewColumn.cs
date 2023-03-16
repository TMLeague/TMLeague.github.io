namespace TMApplication.ViewModels;

public enum ViewColumn
{
    Default,
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
            ViewColumn.Player => players.OrderBy(player => player.Player).SortWithDirection(sortAscending),
            ViewColumn.Points => players.OrderBy(player => player.TotalPoints(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.Wins => players.OrderBy(player => player.Wins(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.Penalties => players.OrderBy(player => player.PenaltiesPoints(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.Cla => players.OrderBy(player => player.Cla(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.Supply => players.OrderBy(player => player.Supplies(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.PT => players.OrderBy(player => player.PowerTokens(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.Baratheon => players.OrderBy(player => player.Baratheon(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.Lannister => players.OrderBy(player => player.Lannister(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.Stark => players.OrderBy(player => player.Stark(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.Tyrell => players.OrderBy(player => player.Tyrell(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.Greyjoy => players.OrderBy(player => player.Greyjoy(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.Martell => players.OrderBy(player => player.Martell(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.Position => players.OrderBy(player => player.Position(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.MPM => players.OrderBy(player => player.MinutesPerMove(scoreType)).SortWithDirection(sortAscending),
            ViewColumn.Battles => players.OrderBy(player => player.Battles(scoreType).Total).SortWithDirection(sortAscending),
            ViewColumn.Kills => players.OrderBy(player => player.Kills(scoreType).Total).SortWithDirection(sortAscending),
            ViewColumn.Casualties => players.OrderBy(player => player.Casualties(scoreType).Total).SortWithDirection(sortAscending),
            ViewColumn.PowerTokensGathered => players.OrderBy(player => player.PowerTokensGathered(scoreType).Total).SortWithDirection(sortAscending),
            ViewColumn.PowerTokensSpent => players.OrderBy(player => player.PowerTokensSpent(scoreType).Total).SortWithDirection(sortAscending),
            ViewColumn.AllSeasons => players.OrderBy(player => player.Seasons).SortWithDirection(sortAscending),
            _ => players
        };

    private static IEnumerable<PlayerScoreViewModel> SortWithDirection(this IOrderedEnumerable<PlayerScoreViewModel> orderBy, bool sortAscending) =>
        sortAscending ? orderBy : orderBy.Reverse();

    public static bool GetSortAscendingDefault(this ViewColumn column) => column switch
    {
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
        ViewColumn.Battles => false,
        ViewColumn.Kills => false,
        ViewColumn.Casualties => true,
        ViewColumn.PowerTokensGathered => false,
        ViewColumn.PowerTokensSpent => false,
        ViewColumn.AllSeasons => false,
        _ => false
    };
}