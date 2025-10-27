namespace MashGamemodeLibrary.Util;

public static class DictionaryExtender
{
    public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> factory) where TKey : notnull
    {
        if (dict.TryGetValue(key, out var value))
            return value;

        var newValue = factory();
        dict[key] = newValue;
        return newValue;
    }
}