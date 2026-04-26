using BoneStrike.Tags;
using BoneStrike.Teams;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.CommonComponents;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Grabbing;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;
using MashGamemodeLibrary.Util.Timer;
using UnityEngine;

namespace BoneStrike.Phase;

internal class FetchClockPacket : INetSerializable, IKnownSenderPacket
{
    public Handedness Hand;
    public byte SenderSmallId { get; set; }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Hand);
    }
}

public class PlantPhase : GamePhase
{
    private static readonly string ClockBarcode = "SLZ.BONELAB.Content.Spawnable.AlarmClock";

    private static readonly SyncedVariable<bool> PhaseShouldQuit = new("SkipPlantPhase", new BoolEncoder(), false, CommonNetworkRoutes.AllToAll);

    private static readonly RemoteEvent<FetchClockPacket> FetchClockEvent = new("FetchClockEvent", OnFetchClock, CommonNetworkRoutes.AllToHost);

    public override string Name => "Plant Phase";
    public override float Duration => BoneStrike.Config.PlantDuration;

    protected override TimeMarker[] Markers => new[]
    {
        CommonTimeMarkerEvents.TimeRemaining(10f),
        CommonTimeMarkerEvents.TimeRemaining(60f)
    };

    public override PhaseIdentifier GetNextPhase()
    {
        if (HasReachedDuration() || PhaseShouldQuit.Value) return PhaseIdentifier.Of<DefusePhase>();

        return PhaseIdentifier.Empty();
    }

    protected override void OnPhaseEnter()
    {
        LocalInventory.SetAmmo(2000);
        // Assign grip check
        PlayerGrabManager.GrabPredicate = GrabPredicate;
        
        Executor.RunIfHost(() =>
        {
            // Only broadcast on the host to prevent spamming packages
            PhaseShouldQuit.Value = false;
            PersistentTeams.AssignAll();
            PalletLoadoutManager.AssignAll();

            var position = BoneStrike.Context.LocalPlayer.RigRefs.RightHand.transform.position;
            GameAssetSpawner.SpawnNetworkAsset(ClockBarcode, position, new BombMarker(), new DefusableTag(), new ImportantEntityMarker());

            BoneStrike.Context.PlantPhaseStartAudioPlayer.PlayRandom();
        });
    }
    protected override void OnPhaseExit()
    {
        PlayerGrabManager.GrabPredicate = null;
    }

    public override void OnPlayerAction(PlayerID playerId, PlayerGameActions action, Handedness handedness)
    {
        if (!playerId.IsMe)
            return;

        if (action != PlayerGameActions.Ability)
            return;

        if (!playerId.IsTeam<TerroristTeam>())
            return;

        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player))
            return;

        if (player.IsHolding<BombMarker>(handedness))
        {
            PhaseShouldQuit.Value = true;
        }
        else
        {
            FetchClockEvent.Call(new FetchClockPacket
            {
                Hand = handedness
            });
        }
    }
    
    private bool GrabPredicate(GrabRequest request)
    {
        var entity = request.GrabbedNetworkEntity;
        if (entity == null)
            return true;
        
        // Don't allow grabbing other players during this phase if they are on the enemy team
        var networkPlayer = entity.GetExtender<NetworkPlayer>();
        if (networkPlayer != null && !networkPlayer.PlayerID.IsTeamMember())
            return false;

        // Bunch of checks for holster grabbing
        var parent = request.GrabbedEntity.transform.parent;
        if (parent == null)
            return true;
        var slot = parent.GetComponent<InventorySlotReceiver>();
        if (slot == null)
            return true;
        if (!InventorySlotReceiverExtender.Cache.TryGet(slot, out var extender))
            return true;
        if (!extender.HasOwner)
            return true;
        if (extender.OwnerID.IsTeamMember())
            return true;

        // Holstered weapon of another player, don't allow grabbing
        return false;
    }

    // Events

    private static void OnFetchClock(FetchClockPacket packet)
    {
        if (!NetworkPlayerManager.TryGetPlayer(packet.SenderSmallId, out var player))
            return;

        if (!player.HasRig)
            return;

        var hand = player.RigRefs.GetHand(packet.Hand);

        if (hand.HasAttachedObject())
            return;

        var position = hand.palmPositionTransform.position;

        var bombs = BombMarker.Query;
        foreach (var entry in bombs)
        {
            var networkEntity = entry.NetworkEntity;
            if (networkEntity == null)
                continue;
            var networkProp = networkEntity.GetExtender<NetworkProp>();
            if (networkProp == null)
                continue;
            var marrow = networkProp.MarrowEntity;
            if (marrow == null)
                continue;
            // If the clock is already held by a player, don't teleport it to the player, as that would cause it to rubberband in their hand
            if (InteractableHost.Cache.TryGet(marrow.gameObject, out var host) && host.IsAttached)
                continue;

            NetworkEntityManager.TakeOwnership(networkEntity);
            networkEntity.LockOwner();
            marrow.EnableColliders(false);
            marrow._anchorBody._rigidbody.velocity = Vector3.zero;
            marrow.Teleport(position, Quaternion.identity, true);
            marrow.EnableColliders();
            networkEntity.UnlockOwner();
        }
    }
}