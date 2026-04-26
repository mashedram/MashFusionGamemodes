using LabFusion.Entities;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.Association.Impl;

public class NetworkEntityAssociation : IEcsAssociation, IEquatable<NetworkEntityAssociation>
{
    public NetworkEntityReference NetworkID;
    
    // Default constructor for deserialization
    public NetworkEntityAssociation() {}
    
    public NetworkEntityAssociation(ushort id)
    {
        NetworkID = new NetworkEntityReference(id);
    }
    
    public NetworkEntityAssociation(NetworkEntityReference reference)
    {
        NetworkID = reference;
    }
    
    public NetworkEntityAssociation(NetworkEntity entity)
    {
        NetworkID = new NetworkEntityReference(entity);
    }

    public void HookReady(Action action)
    {
        // TODO : This might work inconsistently, that's wy WaitOnMarrowEntity exists. Dunno tho
        NetworkID.HookEntityRegistered(_ => action());
    }
    
    public void HookRemoval(Action action)
    {
        if (!NetworkID.TryGetEntity(out var entity))
        {
            InternalLogger.Error($"Failed to fetch entity {NetworkID.ID}, did you call this HookRemoval too early?");
            return;
        }
        
        NetworkEntityDelegate callback = null!;
        callback = _ =>
        {
            action();
            entity.OnEntityUnregistered -= callback; // if this method exists
        };
        entity.OnEntityUnregistered += callback;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref NetworkID);
    }

    public int GetID()
    {
        return NetworkID.ID;
    }
    
    public bool Equals(NetworkEntityAssociation? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return NetworkID.ID == other.NetworkID.ID;
    }
    
    public override int GetHashCode()
    {
        return GetID();
    }
}