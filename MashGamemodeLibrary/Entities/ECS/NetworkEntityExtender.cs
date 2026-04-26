using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Association;
using MashGamemodeLibrary.Entities.Association.Impl;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS;

public static class NetworkEntityExtender
{
    public static void AddComponent(this NetworkEntity entity, IComponent component)
    {
        var association = new NetworkEntityAssociation(entity);
        var index = new EcsIndex(component, association);
        EcsManager.Add(index, component);
    }
    
    public static void RemoveComponent<TComponent>(this NetworkEntity entity) where TComponent : class, IComponent
    {
        var association = new NetworkEntityAssociation(entity);
        var index = new EcsIndex(typeof(TComponent), association);
        EcsManager.Remove(index);
    }
    
    public static TComponent? GetComponent<TComponent>(this NetworkEntity entity) where TComponent : class, IComponent
    {
        var association = new NetworkEntityAssociation(entity);
        var index = new EcsIndex(typeof(TComponent), association);
        return EcsManager.Get<TComponent>(index);
    }

    public static void ClearComponents(this NetworkEntity entity)
    {
        var association = new NetworkEntityAssociation(entity);
        EcsManager.Clear(association);
    }
}