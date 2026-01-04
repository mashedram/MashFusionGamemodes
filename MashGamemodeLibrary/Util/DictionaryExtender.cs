using LabFusion.Extensions;

namespace MashGamemodeLibrary.Util;

public static class DictionaryExtender
{
    public static TValue GetValueOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> factory) where TKey : notnull
    {
        if (dict.TryGetValue(key, out var value))
            return value;

        var newValue = factory();
        dict[key] = newValue;
        return newValue;
    }
    
    public static TValue GetValueOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) 
        where TKey : notnull
        where TValue : new()
    {
        if (dict.TryGetValue(key, out var value))
            return value;

        var newValue = new TValue();
        dict[key] = newValue;
        return newValue;
    }

    public static void Clear<TKey, TValue>(this Dictionary<TKey, TValue> dict, Action<KeyValuePair<TKey, TValue>> onEach) where TKey : notnull
    {
        dict.ForEach(onEach);
        dict.Clear();
    }
    
    public static void Clear<TKey, TValue>(this Dictionary<TKey, TValue> dict, Action<TKey, TValue> onEach) where TKey : notnull
    {
        dict.Clear(kvp => onEach(kvp.Key, kvp.Value));
    }
}