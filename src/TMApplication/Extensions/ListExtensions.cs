namespace TMApplication.Extensions;

public static class ListExtensions
{
    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        for (var i = list.Count - 1; i > 1; i--)
        {
            var k = Random.Shared.Next(i + 1);
            (list[k], list[i]) = (list[i], list[k]);
        }

        return list;
    }
}