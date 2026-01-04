using System.Diagnostics.CodeAnalysis;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.ECS.Caches;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS.Data;

public struct EcsIndex : INetSerializable, IEquatable<EcsIndex>
{
    public NetworkEntityReference EntityID;
    public ulong ComponentID;
    
    public EcsIndex(NetworkEntityReference entityID, ulong componentID)
    {
        EntityID = entityID;
        ComponentID = componentID;
    }
    
    public EcsIndex(ushort entityID, ulong componentID)
    {
        EntityID = new NetworkEntityReference(entityID);
        ComponentID = componentID;
    }
    

    public bool TryGetEntity([MaybeNullWhen(false)] out NetworkEntity networkEntity)
    {
        networkEntity = EntityID.GetEntity();
        return networkEntity != null;
    }

    public int? GetSize()
    {
        return sizeof(ushort) + sizeof(ulong);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EntityID);
        serializer.SerializeValue(ref ComponentID);
    }

    public static EcsIndex Create(NetworkEntity networkEntity, IComponent component)
    {
        var networkId = networkEntity.ID;
        var componentId = LocalEcsCache.Registry.GetID(component);
        
        return new EcsIndex(networkId, componentId);
    }
    
    public bool Equals(EcsIndex other)
    {
        return EntityID.ID.Equals(other.EntityID.ID) && ComponentID == other.ComponentID;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is EcsIndex other && Equals(other);
    }
    
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(EntityID.ID, ComponentID);
    }
}