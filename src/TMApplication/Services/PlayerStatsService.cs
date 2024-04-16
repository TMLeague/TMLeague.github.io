using TMModels;

namespace TMApplication.Services;

public class PlayerStatsService
{
    /// <summary>
    /// A house x house tables with approximate measure of total strength of interactions between them
    /// </summary>
    public static readonly IReadOnlyDictionary<int, double[][]> ProximityScores = new Dictionary<int, double[][]>
    {
        [6] = new[]
        {
            //       n, Barath Lannis Stark  Tyrell Greyjo Martell
            new [] { 0, 0,     0,     0,     0,     0,     0D    }, // Unknown
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
            new [] { 0, 0,    0,    0,    0,    0,    0D   }, // Unknown
            new [] { 0, 0,    -0.5, 0.5,  0,    0,    -0.5 }, // Baratheon
            new [] { 0, -0.5, 0,    0,    0.5,  1,    0D   }, // Lannister
            new [] { 0, 0.5,  0,    0,    0,    -1,   0D   }, // Stark
            new [] { 0, 0,    0.5,  0,    0,    0,    1D   }, // Tyrell
            new [] { 0, 0,    1,   -1,    0,    0,    0D   }, // Greyjoy
            new [] { 0, -0.5, 0,    0,    1,    0,    0D   }  // Martell
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
        var playerRelations = RelationScores.TryGetValue(housesCount, out var relationScores)
            ? playerRows
                .Select(tuple => relationScores[(int)tuple.First][(int)tuple.Second])
                .ToArray()
            : Array.Empty<double>();

        return new PlayerDraftStat(p2,
            playerRows.Count(tuple =>
                tuple.First.IsNeighbor(tuple.Second, housesCount)),
            playerRows.Count(tuple =>
                IsInGame(tuple.First, tuple.Second)),
            playerRows.Sum(tuple =>
                ProximityScore(tuple.First, tuple.Second, housesCount)),
            GetPairs(playerRows, (house1, house2) => house1.IsNeighbor(house2, housesCount)),
            GetPairs(playerRows, IsInGame),
            playerRelations.Where(relation => relation > 0).Sum(),
            -playerRelations.Where(relation => relation < 0).Sum());
    }

    private static int GetPairs((House First, House Second)[] playerRows, Func<House, House, bool> isRelated)
    {
        var playerRelatedRows = playerRows
            .Where(tuple => isRelated(tuple.First, tuple.Second))
            .ToArray();

        var p1Relations = playerRelatedRows
            .ToDictionary(tuple => tuple.First, tuple => tuple.Second);

        return playerRelatedRows.Count(tuple =>
            p1Relations.TryGetValue(tuple.Second, out var p2House) &&
            p2House == tuple.First);
    }

    private static bool IsInGame(House playerHouse, House otherHouse) =>
        playerHouse != House.Unknown && otherHouse != House.Unknown && otherHouse != playerHouse;

    private static double ProximityScore(House playerHouse, House otherHouse, int housesCount) =>
        ProximityScores.TryGetValue(housesCount, out var proximityScores) ?
            proximityScores[(int)playerHouse][(int)otherHouse] :
            playerHouse.IsNeighbor(otherHouse, housesCount) ? 1 : 0;
}