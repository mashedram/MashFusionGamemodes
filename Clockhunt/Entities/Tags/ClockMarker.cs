using Clockhunt.Phase;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.ECS.Query;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Phase;

namespace Clockhunt.Entities.Tags;

public class ClockMarker : IComponent, IGrabPredicate, INetSerializable
{
    public static readonly CachedQuery<ClockMarker> Query = EcsManager.CacheQuery<ClockMarker>();
    
    public byte OwnerId;

    public ClockMarker()
    {
    }

    public ClockMarker(NetworkPlayer player)
    {
        OwnerId = player.PlayerID.SmallID;
    }

    public NetworkPlayer? Owner => NetworkPlayerManager.TryGetPlayer(OwnerId, out var player) ? player : null;

    public bool CanGrab(GrabData grabData)
    {
        if (!GamePhaseManager.IsPhase<HidePhase>()) return true;

        return grabData.NetworkPlayer?.PlayerID == OwnerId;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref OwnerId);
    }
}