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
        return players.Select((p1, p1Idx) => 
            new PlayerDraftStats(players.Select((p2, p2Idx) => 
                GetPlayerDraftStat(draftTable, p1, p1Idx, p2, p2Idx, housesCount))));
    }

    private static PlayerDraftStat? GetPlayerDraftStat(House[][] draftTable, string p1, int p1Idx, string p2, int p2Idx,
        int housesCount)
    {
        if (p1 == p2) return null;
        var p1Row = draftTable[p1Idx];
        var p2Row = draftTable[p2Idx];
        return new PlayerDraftStat(p2,
            p1Row
                .Zip(p2Row)
                .Sum(tuple =>
                    IsNeighbor(tuple.First, tuple.Second, housesCount) ? 1 : 0),
            p1Row
                .Zip(p2Row)
                .Sum(tuple =>
                    IsEnemy(tuple.First, tuple.Second) ? 1 : 0));
    }


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