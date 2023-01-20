using TMModels;

namespace TMApplication.Services;

public class DraftService
{
    public Draft GetDraft(int players)
    {
        return new Draft("Random", Enumerable.Repeat(new string[players], players).ToArray());
    }

    public Draft GetDraft(Draft draft)
    {
        var playersCount = draft.Table.Length;
        if (playersCount == 0)
            return draft;

        var gamesCount = draft.Table[0].Length;

        var playersRandom = Shuffle(Enumerable.Range(0, playersCount).ToList());
        var gamesRandom = Shuffle(Enumerable.Range(0, gamesCount).ToList());

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

    private static IList<T> Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 1; i--)
        {
            var k = Random.Shared.Next(i + 1);
            (list[k], list[i]) = (list[i], list[k]);
        }

        return list;
    }
}