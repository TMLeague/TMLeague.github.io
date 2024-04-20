namespace TMModels.Extensions;
internal static class DictionaryExtensions
{
    public static Dictionary<TKey, TValue> OuterJoin<TKey, TValue>(this IDictionary<TKey, TValue> dict1,
        IDictionary<TKey, TValue> dict2, Func<TValue, TValue, TValue> function)
        where TValue : new()
        where TKey : notnull =>
        dict1.Keys.Concat(dict2.Keys).Distinct()
            .ToDictionary(key => key, key =>
            {
                var are1 = dict1.TryGetValue(key, out var value1);
                var are2 = dict2.TryGetValue(key, out var value2);
                return are1 && are2
                    ? function(value1!, value2!)
                    : value1 ?? value2 ?? new TValue();
            });
}
