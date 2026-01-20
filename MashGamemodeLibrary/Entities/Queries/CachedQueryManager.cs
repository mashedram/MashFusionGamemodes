using System.Reflection;
using System.Runtime.CompilerServices;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.Queries;

public static class CachedQueryManager
{
    private static readonly Dictionary<Type, ICachedQuery> Queries = new();

    public static CachedQuery<T> Create<T>()
    {
        return (CachedQuery<T>) Queries.GetValueOrCreate(typeof(T), () => new CachedQuery<T>());
    }
    
    internal static CacheKey? Add(object queryable)
    {
        return Queries.GetValueOrDefault(queryable.GetType())?.TryAdd(queryable);
    }
}