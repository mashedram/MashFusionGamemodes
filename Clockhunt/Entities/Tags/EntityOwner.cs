using Clockhunt.Phase;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Components;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Phase;

namespace Clockhunt.Entities.Tags;

public class EntityOwner : EntityTag, IEntityGrabPredicate, INetSerializable
{
    public byte OwnerId;

    public EntityOwner()
    {
    }

    public EntityOwner(NetworkPlayer player)
    {
        OwnerId = player.PlayerID.SmallID;
    }

    public NetworkPlayer? NetworkPlayer => NetworkPlayerManager.TryGetPlayer(OwnerId, out var player) ? player : null;

    public bool CanGrab(GrabData grabData)
    {
        if (!GamePhaseManager.IsPhase<HidePhase>()) return true;

        return grabData.NetworkPlayer.PlayerID == OwnerId;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref OwnerId);
    }
}