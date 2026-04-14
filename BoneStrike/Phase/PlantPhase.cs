using BoneStrike.Tags;
using BoneStrike.Teams;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Tagging;
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

internal class TeleportClockRequest : INetSerializable, IKnownSenderPacket
{
    public ushort ClockNetworkId;
    public byte SenderSmallId { get; set; }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ClockNetworkId);
    }
}

public class PlantPhase : GamePhase
{
    private static readonly string ClockBarcode = "SLZ.BONELAB.Content.Spawnable.AlarmClock";

    public static readonly SyncedVariable<bool> PhaseShouldQuit = new("SkipPlantPhase", new BoolEncoder(), false, CommonNetworkRoutes.AllToAll);

    private static readonly RemoteEvent<FetchClockPacket> FetchClockEvent = new("FetchClockEvent", OnFetchClock, CommonNetworkRoutes.AllToHost);
    private static readonly RemoteEvent<TeleportClockRequest> TeleportClockEvent = new("TeleportClockEvent", OnTeleportClock, CommonNetworkRoutes.HostToRemote);
    
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
        PhaseShouldQuit.Value = false;
        LocalInventory.SetAmmo(2000);
        Executor.RunIfHost(() =>
        {
            BoneStrike.Context.PersistentTeams.AssignAll();
            PalletLoadoutManager.AssignAll();

            var position = BoneStrike.Context.LocalPlayer.RigRefs.RightHand.transform.position;
            GameAssetSpawner.SpawnNetworkAsset(ClockBarcode, position, new BombMarker(), new DefusableTag(), new IsImportantTag());
            
            BoneStrike.Context.PlantPhaseStartAudioPlayer.PlayRandom();
        });
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

        if (PlayerGrabManager.IsHolding<BombMarker>(player.RigRefs.GetHand(handedness)))
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

    // Events

    private static void OnFetchClock(FetchClockPacket packet)
    {
        if (!NetworkPlayerManager.TryGetPlayer(packet.SenderSmallId, out var player))
            return;

        if (!player.HasRig)
            return;

        var bombs = BombMarker.Query;
        foreach (var entry in bombs)
        {
            var networkEntity = entry.NetworkEntity;
            if (networkEntity == null)
                continue;

            if (networkEntity.OwnerID.IsMe)
            {
                TeleportClock(player, networkEntity);
            }
            else
            {
                TeleportClockEvent.CallFor(networkEntity.OwnerID, new TeleportClockRequest
                {
                    ClockNetworkId = networkEntity.ID
                });
            }
        }
    }

    private static void TeleportClock(NetworkPlayer networkPlayer, NetworkEntity networkEntity)
    {
        if (!networkPlayer.HasRig)
            return;
        
        var hand = networkPlayer.RigRefs.GetHand(Handedness.RIGHT);
        if (hand.HasAttachedObject())
            return;
        
        var networkProp = networkEntity.GetExtender<NetworkProp>();
        if (networkProp == null)
            return;
        
        var marrowEntity = networkProp.MarrowEntity;
        if (marrowEntity == null)
            return;

        if (marrowEntity.IsCulled)
        {
            marrowEntity.ClearHibernation();
        }
        
        var position = hand.transform.position;
        marrowEntity.Teleport(position, Quaternion.identity, true);
    }
    
    private static void OnTeleportClock(TeleportClockRequest packet)
    {
        if (!NetworkPlayerManager.TryGetPlayer(packet.SenderSmallId, out var player))
            return;
        
        var clockEntity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(packet.ClockNetworkId);   
        if (clockEntity == null)
            return;
        
        TeleportClock(player, clockEntity);
    }
}