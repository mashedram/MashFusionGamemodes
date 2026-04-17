using System.Collections;

namespace MashGamemodeLibrary.Entities.Queries;

internal record struct KeyComponentPair<TKey, TValue>(TKey Key, TValue Value);

public class KeyedCachedQuery<TKey, TValue> : ICachedQuery, IEnumerable<TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _keys = new();
    private readonly Dictionary<Guid, KeyComponentPair<TKey, TValue>> _components = new();
    private readonly Func<TValue, TKey> _fetcher;

    public KeyedCachedQuery(Func<TValue, TKey> fetcher)
    {
        _fetcher = fetcher;
    }

    /// <summary>
    /// FOR INTERNAL USE ONLY
    /// </summary>
    public CacheKey? TryAdd(object instance)
    {
        if (instance is not TValue typedInstance)
            return null;

        var key = Guid.NewGuid();
        var customKey = _fetcher(typedInstance);
        _components.Add(key, new KeyComponentPair<TKey, TValue>(customKey, typedInstance));

        _keys.Add(customKey, typedInstance);
        
        return new CacheKey(this, key);
    }

    /// <summary>
    /// FOR INTERNAL USE ONLY
    /// </summary>
    public void Remove(CacheKey key)
    {
        if (_components.Remove(key.Guid, out var pair))
        {
            _keys.Remove(pair.Key);
        }
    }

    public IEnumerator<TValue> GetEnumerator()
    {
        return _keys.Values.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}