using System.Collections;
using MashGamemodeLibrary.Entities.ECS.Caches;
using MashGamemodeLibrary.Entities.ECS.Data;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS.Query;

internal interface ICachedQuery
{
    void TryAdd(ComponentInstance instance);
    void Remove(EcsIndex ecsIndex);
}

public record QueryEntry<T>(ComponentInstance Instance, T Component);

public class CachedQuery<T> : ICachedQuery, IEnumerable<QueryEntry<T>> where T : IComponent
{
    private Dictionary<EcsIndex, QueryEntry<T>> _components = new();

    /// <summary>
    /// FOR INTERNAL USE ONLY
    /// </summary>
    public void TryAdd(ComponentInstance instance)
    {
        if (!instance.TryGetAs<T>(out var component))
            return;
        
        _components.Add(instance.Index, new QueryEntry<T>(instance, component));
    }

    /// <summary>
    /// FOR INTERNAL USE ONLY
    /// </summary>
    public void Remove(EcsIndex ecsIndex)
    {
        _components.Remove(ecsIndex);
    }
    
    public IEnumerator<QueryEntry<T>> GetEnumerator()
    {
        return _components.Values.Where(q => q.Instance.IsReady).GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}