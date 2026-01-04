using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.ECS.Caches;
using MashGamemodeLibrary.Entities.ECS.Data;

namespace MashGamemodeLibrary.Entities.ECS.Integration;

public class ECSExtender : IEntityExtender
{
    private NetworkEntityReference _entityReferenceReference;
    private NetworkEntity? _networkEntity;
    private Dictionary<ulong, ComponentInstance> _components = new();
    
    internal ECSExtender(NetworkEntityReference entityReference)
    {
        _entityReferenceReference = entityReference;
        _entityReferenceReference.HookEntityRegistered(OnRegistered);
    }

    private void OnRegistered(NetworkEntity entity)
    {
        _networkEntity = entity;
        _networkEntity.OnEntityUnregistered += OnUnregistered;
        
        _networkEntity.ConnectExtender(this);
    }
    
    // Not for direct use, use: GlobalECSCache.AddComponent
    internal void AddComponent(ComponentInstance componentInstance)
    {
        _components.Add(componentInstance.Index.ComponentID, componentInstance);
    }

    internal void RemoveComponent(EcsIndex ecsIndex)
    {
        _components.Remove(ecsIndex.ComponentID);
    }
    
    private void OnUnregistered(NetworkEntity entity)
    {
        Disconnect();
    }

    internal bool IsEmpty()
    {
        return _components.Count == 0;
    }

    internal void Disconnect()
    {
        if (_networkEntity == null)
            return;
        
        _networkEntity.DisconnectExtender(this);
        _networkEntity = null;
    }
}