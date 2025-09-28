using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.Tagging.Tags;

namespace Clockhunt.Entities.Tags;

public class EntityOwner : IEntityTag, INetSerializable
{
    public byte OwnerId;
    public NetworkPlayer? NetworkPlayer => NetworkPlayerManager.TryGetPlayer(OwnerId, out var player) ? player : null;

    public EntityOwner()
    {
        
    }
    
    public EntityOwner(NetworkPlayer player)
    {
        OwnerId = player.PlayerID.SmallID;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref OwnerId);
    }
}