namespace TMModels.Extensions;

public static class LinqExtensions
{
    public static int GetIdx<T>(this T[] array, T value) =>
        Array.IndexOf(array, value);

    public static int GetIdx<T>(this T[] array, T value, int startIndex) =>
        Array.IndexOf(array, value, startIndex);

    public static int GetIdx<T>(this T[] array, T value, int startIndex, int count) =>
        Array.IndexOf(array, value, startIndex, count);
}