using System.Collections;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.ECS.Data;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.ECS.Caches;

public interface IEscTargetedCache
{
    Type Target { get; }

    void Add(EcsIndex index, ComponentInstance instance);
    void Remove(EcsIndex index);
    void Clear();
}

public class EcsBehaviourCache<T> : BehaviourCache<EcsIndex, ComponentInstance, T> ,IEscTargetedCache where T : IBehaviour
{
    public void Add(EcsIndex index, ComponentInstance instance)
    {
        if (!instance.TryGetAs<T>(out var component))
            throw new Exception($"Failed to add tag of type: {instance.GetType().FullName} to cache containing type: {instance.GetType().FullName}");
        
        Add(index, instance, component);
    }
}