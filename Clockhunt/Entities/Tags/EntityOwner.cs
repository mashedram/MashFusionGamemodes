using Clockhunt.Phase;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.Interaction.Components;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Phase;

namespace Clockhunt.Entities.Tags;

public class EntityOwner : IEntityTag, IEntityGrabPredicate, INetSerializable
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

    public bool CanGrab(NetworkPlayer grabber, NetworkEntity entity, MarrowEntity marrowEntity)
    {
        if (!GamePhaseManager.IsPhase<HidePhase>())
        {
            return true;
        }
        
        return grabber.PlayerID.SmallID == OwnerId;
    }
}