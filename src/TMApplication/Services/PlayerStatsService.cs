using TMModels;

namespace TMApplication.Services;

public class PlayerStatsService
{
    /// <summary>
    /// A dictionaries of houses that are neighboring to each other.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, Dictionary<House, House[]>> Neighbors = new Dictionary<int, Dictionary<House, House[]>>
    {
        [3] = new()
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark },
            [House.Lannister] = new[] { House.Baratheon, House.Stark },
            [House.Stark] = new[] { House.Baratheon, House.Lannister }
        },
        [4] = new()
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark, House.Tyrell },
            [House.Lannister] = new[] { House.Baratheon, House.Greyjoy, House.Tyrell },
            [House.Stark] = new[] { House.Baratheon, House.Greyjoy },
            [House.Tyrell] = new[] { House.Baratheon, House.Lannister },
            [House.Greyjoy] = new[] { House.Lannister, House.Stark }
        },
        [5] = new()
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark, House.Tyrell },
            [House.Lannister] = new[] { House.Baratheon, House.Greyjoy, House.Tyrell },
            [House.Stark] = new[] { House.Baratheon, House.Greyjoy },
            [House.Tyrell] = new[] { House.Baratheon, House.Lannister },
            [House.Greyjoy] = new[] { House.Lannister, House.Stark }
        },
        [6] = new()
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark, House.Martell }, // no Tyrell
            [House.Lannister] = new[] { House.Baratheon, House.Greyjoy, House.Tyrell },
            [House.Stark] = new[] { House.Baratheon, House.Greyjoy },
            [House.Tyrell] = new[] { House.Lannister, House.Martell }, // no Baratheon
            [House.Greyjoy] = new[] { House.Lannister, House.Stark },
            [House.Martell] = new[] { House.Baratheon, House.Tyrell }
        },
        [7] = new()
        {
            [House.Baratheon] = new[] { House.Lannister, House.Stark, House.Martell, House.Arryn },
            [House.Lannister] = new[] { House.Baratheon, House.Greyjoy, House.Tyrell },
            [House.Stark] = new[] { House.Baratheon, House.Greyjoy, House.Arryn },
            [House.Tyrell] = new[] { House.Lannister, House.Martell },
            [House.Greyjoy] = new[] { House.Lannister, House.Stark, House.Arryn },
            [House.Martell] = new[] { House.Baratheon, House.Tyrell },
            [House.Arryn] = new[] { House.Baratheon, House.Stark, House.Greyjoy }
        }
    };

    /// <summary>
    /// A house x house tables with approximate measure of total strength of interactions between them
    /// </summary>
    public static readonly IReadOnlyDictionary<int, double[][]> ProximityScores = new Dictionary<int, double[][]>
    {
        [6] = new[]
        {
            //       n, Barath Lannis Stark  Tyrell Greyjo Martell
            new [] { 0, 0,     0,     0,     0,     0,     0.0   }, // Unknown
            new [] { 0, 0,     0.713, 0.88,  0.55,  0.084, 0.925 }, // Baratheon
            new [] { 0, 0.713, 0,     0.25,  0.838, 1,     0.125 }, // Lannister
            new [] { 0, 0.88,  0.25,  0,     0.163, 0.97,  0.063 }, // Stark
            new [] { 0, 0.55,  0.838, 0.163, 0,     0.45,  0.995 }, // Tyrell
            new [] { 0, 0.084, 1,     0.97,  0.45,  0,     0.04  }, // Greyjoy
            new [] { 0, 0.925, 0.125, 0.063, 0.995, 0.04,  0     }  // Martell
        }
    };

    /// <summary>
    /// A house x house tables with estimations of relations between them:
    /// <list type="bullet">
    ///     <item>1 - strong ally</item>
    ///     <item>0.5 - soft ally</item>
    ///     <item>0 - neutral</item>
    ///     <item>-0.5 - soft enemy</item>
    ///     <item>-1 - strong enemy</item>
    /// </list>
    /// </summary>
    public static readonly IReadOnlyDictionary<int, double[][]> RelationScores = new Dictionary<int, double[][]>
    {
        [6] = new[]
        {
            //       n, Barat Lanni Stark Tyrel Greyj Martell
            new [] { 0, 0,    0,    0,    0,    0,    0.0  }, // Unknown
            new [] { 0, 0,    -0.5, 0.5,  0,    0,    -0.5 }, // Baratheon
            new [] { 0, -0.5, 0,    0,    0.5,  1,    0.0  }, // Lannister
            new [] { 0, 0.5,  0,    0,    0,    -1,   0.0  }, // Stark
            new [] { 0, 0,    0.5,  0,    0,    0,    1.0  }, // Tyrell
            new [] { 0, 0,    1,   -1,    0,    0,    0.0  }, // Greyjoy
            new [] { 0, -0.5, 0,    0,    1,    0,    0.0  }  // Martell
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
        var playerRows = p1Row.Zip(p2Row).ToArray();
        return new PlayerDraftStat(p2,
            playerRows.Count(tuple =>
                IsNeighbor(tuple.First, tuple.Second, housesCount)),
            playerRows.Count(tuple =>
                IsEnemy(tuple.First, tuple.Second)),
            playerRows.Sum(tuple =>
                ProximityScore(tuple.First, tuple.Second, housesCount)),
            GetPairs(playerRows, (house1, house2) => IsNeighbor(house1, house2, housesCount)),
            GetPairs(playerRows, IsEnemy));
    }

    private static int GetPairs((House First, House Second)[] playerRows, Func<House, House, bool> isRelated)
    {
        var playerRelatedRows = playerRows
            .Where(tuple => isRelated(tuple.First, tuple.Second))
            .ToArray();

        var p1Relations = playerRelatedRows
            .ToDictionary(tuple => tuple.First, tuple => tuple.Second);

        return playerRelatedRows.Count(tuple => p1Relations[tuple.Second] == tuple.First);
    }

    private static bool IsEnemy(House playerHouse, House enemyHouse) =>
        playerHouse != House.Unknown && enemyHouse != House.Unknown && enemyHouse != playerHouse;

    private static bool IsNeighbor(House playerHouse, House enemyHouse, int housesCount) =>
        playerHouse != House.Unknown && Neighbors[GetHousesCount(housesCount)][playerHouse].Contains(enemyHouse);

    private static double ProximityScore(House playerHouse, House enemyHouse, int housesCount) =>
        ProximityScores.TryGetValue(housesCount, out var proximityScores) ?
            proximityScores[(int)playerHouse][(int)enemyHouse] :
            IsNeighbor(playerHouse, enemyHouse, housesCount) ? 1 : 0;

    private static int GetHousesCount(int housesCount) => housesCount switch
    {
        < 3 => 3,
        > 7 => 7,
        _ => housesCount
    };
}