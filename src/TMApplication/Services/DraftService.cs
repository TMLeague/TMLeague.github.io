using Microsoft.Extensions.Logging;
using TMApplication.Extensions;
using TMModels;

namespace TMApplication.Services;

public class DraftService
{
    private ILogger<DraftService> _logger;

    public DraftService(ILogger<DraftService> logger)
    {
        _logger = logger;
    }

    public Draft GetDraft(Draft draft)
    {
        var playersCount = draft.Table.Length;
        if (playersCount == 0)
            return draft;

        var gamesCount = draft.Table[0].Length;

        var playersRandom = Enumerable.Range(0, playersCount).ToList().Shuffle();
        var gamesRandom = Enumerable.Range(0, gamesCount).ToList().Shuffle();

        var table = Enumerable.Repeat(new string[gamesCount], playersCount).ToArray();
        for (var playerIdx = 0; playerIdx < playersCount; playerIdx++)
        {
            var games = new string[gamesCount];
            for (var gameIdx = 0; gameIdx < gamesCount; gameIdx++)
                games[gameIdx] = draft.Table[playersRandom[playerIdx]][gamesRandom[gameIdx]];

            table[playerIdx] = games;
        }

        return new Draft($"{draft.Name}*", table);
    }

    public Draft? GetDraft(int playersCount, int housesCount) =>
        GetDrafts(playersCount, housesCount).FirstOrDefault();

    public IEnumerable<Draft> GetDrafts(int playersCount, int housesCount)
    {
        if (playersCount < 3) playersCount = 3;
        if (housesCount < 3) housesCount = 3;
        if (housesCount > 7) housesCount = 7;
        if (housesCount > playersCount) housesCount = playersCount;
        var houses = housesCount switch
        {
            4 => new[] { House.Unknown, House.Baratheon, House.Lannister, House.Stark, House.Greyjoy },
            5 => new[] { House.Unknown, House.Baratheon, House.Lannister, House.Stark, House.Greyjoy, House.Tyrell },
            6 => new[] { House.Unknown, House.Baratheon, House.Lannister, House.Stark, House.Greyjoy, House.Tyrell, House.Martell },
            7 => new[] { House.Unknown, House.Baratheon, House.Lannister, House.Stark, House.Greyjoy, House.Tyrell, House.Martell, House.Arryn },
            _ => new[] { House.Unknown, House.Baratheon, House.Lannister, House.Stark }
        };
        var remainingHouses = Enumerable.Repeat(1, playersCount + 1).ToArray();
        remainingHouses[0] = playersCount - housesCount;

        var players = Enumerable.Range(0, playersCount).Select(_ => remainingHouses.ToArray()).ToArray();
        var games = Enumerable.Range(0, playersCount).Select(_ => remainingHouses.ToArray()).ToArray();
        var table = Enumerable.Range(0, playersCount).Select(_ => new House?[playersCount]).ToArray();

        foreach (var draft in Generate(table, players, games, houses, 0))
            yield return draft;
    }

    private IEnumerable<Draft> Generate(IReadOnlyList<House?[]> table, IReadOnlyList<int[]> players, IReadOnlyList<int[]> games, House[] houses, int idx)
    {
        var gameIdx = idx % games.Count;
        var playerIdx = idx / games.Count;
        if (playerIdx >= players.Count)
        {
            yield return new Draft("Random",
                table.Select(hs =>
                    hs.Select(h => h?.ToString() ?? "X")
                        .ToArray()).ToArray());
        }
        else
        {
            var playerRemaining = players[playerIdx];
            var gameRemaining = games[gameIdx];

            var options = GetRandomOptions(playerRemaining, gameRemaining, houses);
            foreach (var (option, i) in options.Select((o, i) => (o, i)))
            {
                table[playerIdx][gameIdx] = option;
                playerRemaining[Array.IndexOf(houses, option)]--;
                gameRemaining[Array.IndexOf(houses, option)]--;

                foreach (var result in Generate(table, players, games, houses, idx + 1))
                    yield return result;

                playerRemaining[Array.IndexOf(houses, option)]++;
                gameRemaining[Array.IndexOf(houses, option)]++;

                _logger.LogTrace($"Option[{i}] for idx={idx} calculated.");
            }
        }
    }

    private static IEnumerable<House> GetRandomOptions(IReadOnlyList<int> playerRemaining, IReadOnlyList<int> gameRemaining, IEnumerable<House> houses)
    {
        var options = houses.Where((_, houseIdx) =>
            playerRemaining[houseIdx] > 0 &&
            gameRemaining[houseIdx] > 0).ToList();

        var remainingUnknowns = Math.Min(playerRemaining[0], gameRemaining[0]) - 1;
        if (remainingUnknowns > 0)
            options.AddRange(Enumerable.Repeat(House.Unknown, remainingUnknowns));

        return options.Shuffle().Distinct();
    }
}