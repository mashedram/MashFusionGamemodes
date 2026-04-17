using System.Reflection;
using System.Runtime.CompilerServices;
using MashGamemodeLibrary.Entities.ECS.Caches;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.Queries;

public static class CachedQueryManager
{
    private static readonly Dictionary<Type, ICachedQuery> Queries = new();

    private static CachedQuery<T> CreateCache<T>()
    {
        var cache = new CachedQuery<T>();

        // TODO: Load all queryables of this type from the assemblies, and add them to the cache

        return cache;
    }

    private static KeyedCachedQuery<TKey, TValue> CreateKeyedCache<TKey, TValue>(Func<TValue, TKey> fetcher) where TKey : notnull
    {
        var cache = new KeyedCachedQuery<TKey, TValue>(fetcher);
        
        // TODO: Load all queryables of this type from the assemblies, and add them to the cache

        return cache;
    }

    public static CachedQuery<T> Create<T>()
    {
        return (CachedQuery<T>)Queries.GetValueOrCreate(typeof(T), CreateCache<T>);
    }

    public static KeyedCachedQuery<TKey, TValue> Create<TKey, TValue>(Func<TValue, TKey> fetcher) where TKey : notnull
    {
        // TODO : There is a bug here if two keyed caches with the same value but a different key get made
        return (KeyedCachedQuery<TKey, TValue>)Queries.GetValueOrCreate(typeof(TValue), () => CreateKeyedCache<TKey, TValue>(fetcher));
    }

    internal static CacheKey? Add(object queryable)
    {
        return Queries.GetValueOrDefault(queryable.GetType())?.TryAdd(queryable);
    }
}