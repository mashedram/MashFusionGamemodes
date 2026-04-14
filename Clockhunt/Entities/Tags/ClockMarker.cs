using Clockhunt.Phase;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Queries;
using MashGamemodeLibrary.Phase;

namespace Clockhunt.Entities.Tags;

public class ClockMarker : IComponentReady, IGrabPredicate, INetSerializable
{
    public static readonly CachedQuery<ClockMarker> Query = CachedQueryManager.Create<ClockMarker>();

    public NetworkEntity? NetworkEntity;
    public MarrowEntity? MarrowEntity;
    public byte OwnerId;
    
    // Default constructor for serialization
    public ClockMarker() { }

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

    public void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        NetworkEntity = networkEntity;
        MarrowEntity = marrowEntity;
    }
}