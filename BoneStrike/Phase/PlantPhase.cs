using BoneStrike.Tags;
using BoneStrike.Teams;
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

public class PlantPhase : GamePhase
{
    private static readonly string ClockBarcode = "SLZ.BONELAB.Content.Spawnable.AlarmClock";

    public static readonly SyncedVariable<bool> PhaseShouldQuit = new("SkipPlantPhase", new BoolEncoder(), false, CommonNetworkRoutes.AllToAll);

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
        PhaseShouldQuit.Value = false;
        LocalInventory.SetAmmo(2000);
        Executor.RunIfHost(() =>
        {
            PalletLoadoutManager.AssignAll();

            var position = BoneStrike.Context.LocalPlayer.RigRefs.RightHand.transform.position;
            GameAssetSpawner.SpawnNetworkAsset(ClockBarcode, position, new BombMarker(), new DefusableTag());
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

        if (PlayerGrabManager.IsHoldingTag<BombMarker>(player.RigRefs.GetHand(handedness)))
        {
            if (ElapsedTime < 15f)
                return;

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

        var hand = player.RigRefs.GetHand(packet.Hand);

        if (hand.HasAttachedObject())
            return;
        
        var position = hand.palmPositionTransform.position;

        var bombs = EntityTagManager.GetAllWithTag<BombMarker>();
        foreach (var networkEntity in bombs)
        {
            var grip = networkEntity.GetExtender<GripExtender>();
            if (grip == null)
                continue;

            if (grip.Components.Any(g => g.HasAttachedHands()))
                continue;


            var marrow = grip.Components.FirstOrDefault()?._marrowEntity;
            if (marrow == null)
                continue;

            NetworkEntityManager.TakeOwnership(networkEntity);
            marrow.Teleport(position, Quaternion.identity, true);
        }
    }
}