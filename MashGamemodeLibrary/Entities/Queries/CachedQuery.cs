using System.Collections;
using MashGamemodeLibrary.Entities.ECS.Data;

namespace MashGamemodeLibrary.Entities.Queries;

public interface ICachedQuery
{
    CacheKey? TryAdd(object instance);
    void Remove(CacheKey key);
}

public class CachedQuery<T> : ICachedQuery, IEnumerable<T>
{
    private Dictionary<Guid, T> _components = new();

    /// <summary>
    /// FOR INTERNAL USE ONLY
    /// </summary>
    public CacheKey? TryAdd(object instance)
    {
        if (instance is not T typedInstance)
            return null;

        var key = Guid.NewGuid();
        _components.Add(key, typedInstance);
        return new CacheKey(this, key);
    }

    /// <summary>
    /// FOR INTERNAL USE ONLY
    /// </summary>
    public void Remove(CacheKey key)
    {
        _components.Remove(key.Guid);
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        return _components.Values.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}