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

    public static void Register<T>()
    {
        var assembly = typeof(T).Assembly;
        var queryTypes = assembly
            .GetTypes()
            .Where(t => t.GetFields().Any(f => typeof(ICachedQuery).IsAssignableFrom(f.FieldType)));
    
        foreach (var type in queryTypes)
        {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
    } 
    
    internal static CacheKey? Add(object queryable)
    {
        return Queries.GetValueOrDefault(queryable.GetType())?.TryAdd(queryable);
    }
}