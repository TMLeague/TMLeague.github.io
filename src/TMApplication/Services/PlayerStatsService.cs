using TMLeague.Models.Judge;
using TMModels;

namespace TMApplication.Services;

public class PlayerStatsService
{
    public static readonly Dictionary<int, Dictionary<House, House[]>> Neighbors = new()
    {
        [3] = new Dictionary<House, House[]>
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark },
            [House.Lannister] = new[] { House.Baratheon, House.Stark },
            [House.Stark] = new[] { House.Baratheon, House.Lannister }
        },
        [4] = new Dictionary<House, House[]>
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark, House.Tyrell },
            [House.Lannister] = new[] { House.Baratheon, House.Greyjoy, House.Tyrell },
            [House.Stark] = new[] { House.Baratheon, House.Greyjoy },
            [House.Greyjoy] = new[] { House.Lannister, House.Stark },
            [House.Tyrell] = new[] { House.Baratheon, House.Lannister }
        },
        [5] = new Dictionary<House, House[]>
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark, House.Tyrell },
            [House.Lannister] = new[] { House.Baratheon, House.Greyjoy, House.Tyrell },
            [House.Stark] = new[] { House.Baratheon, House.Greyjoy },
            [House.Greyjoy] = new[] { House.Lannister, House.Stark },
            [House.Tyrell] = new[] { House.Baratheon, House.Lannister }
        },
        [6] = new Dictionary<House, House[]>
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark, House.Tyrell, House.Martell },
            [House.Lannister] = new[] { House.Baratheon, House.Greyjoy, House.Tyrell },
            [House.Stark] = new[] { House.Baratheon, House.Greyjoy },
            [House.Greyjoy] = new[] { House.Lannister, House.Stark },
            [House.Tyrell] = new[] { House.Baratheon, House.Lannister, House.Martell },
            [House.Martell] = new[] { House.Baratheon, House.Tyrell }
        },
        [7] = new Dictionary<House, House[]>
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark, House.Tyrell, House.Martell, House.Arryn },
            [House.Lannister] = new[] { House.Baratheon, House.Greyjoy, House.Tyrell },
            [House.Stark] = new[] { House.Baratheon, House.Greyjoy, House.Arryn },
            [House.Greyjoy] = new[] { House.Lannister, House.Stark, House.Arryn },
            [House.Tyrell] = new[] { House.Baratheon, House.Lannister, House.Martell },
            [House.Martell] = new[] { House.Baratheon, House.Tyrell },
            [House.Arryn] = new[] { House.Baratheon, House.Stark, House.Greyjoy }
        }
    };

    public IEnumerable<PlayerDraftStats> GetStats(House[][] draftTable, string[] players)
    {
        var houses = draftTable.First().Distinct().OrderBy(house => house).ToArray();
        var housesCount = houses.Length - (houses.Contains(House.Unknown) ? 1 : 0);
        var enemies = GetPlayersDraftStats(draftTable, players,
            IsEnemy);
        var neighbors = GetPlayersDraftStats(draftTable, players,
            (playerHouse, enemyHouse) => IsNeighbor(playerHouse, enemyHouse, housesCount));
        return enemies.Zip(neighbors)
            .Select(tuple => new PlayerDraftStats(
                tuple.First.ToList(),
                tuple.Second.ToList()));
    }

    private static IEnumerable<IEnumerable<PlayerDraftStat>> GetPlayersDraftStats(House[][] draftTable, IReadOnlyList<string> players, Func<House, House, bool> isRelated) =>
        draftTable
            .Select(playerGames => SelectAllRelated(playerGames, draftTable, isRelated)
                .GroupBy(enemyIdx => enemyIdx)
                .Select(enemyIdxGrouping =>
                    new PlayerDraftStat(players[enemyIdxGrouping.Key],
                        enemyIdxGrouping.Count())));

    private static IEnumerable<int> SelectAllRelated(IEnumerable<House> playerGames, House[][] draftTable, Func<House, House, bool> isRelated) =>
        playerGames
            .SelectMany((playerHouse, gameIdx) => draftTable
                .Select((enemyGames, enemyIdx) => (enemyGames, enemyIdx))
                .Where(kv => isRelated(playerHouse, kv.enemyGames[gameIdx]))
                .Select(kv => kv.enemyIdx));

    private static bool IsEnemy(House playerHouse, House enemyHouse) =>
        playerHouse != House.Unknown && enemyHouse != House.Unknown && enemyHouse != playerHouse;

    private static bool IsNeighbor(House playerHouse, House enemyHouse, int housesCount) =>
        playerHouse != House.Unknown && Neighbors[GetHousesCount(housesCount)][playerHouse].Contains(enemyHouse);

    private static int GetHousesCount(int housesCount) => housesCount switch
    {
        < 3 => 3,
        > 7 => 7,
        _ => housesCount
    };
}