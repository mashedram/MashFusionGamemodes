using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Entities.Tags;
using Clockhunt.Game;
using Clockhunt.Game.Player;
using Clockhunt.Game.Teams;
using Clockhunt.Joke;
using Clockhunt.Nightmare;
using Clockhunt.Vision;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Integration;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.SDK.Gamemodes;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using MashGamemodeLibrary.Data.Random;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Util;
using MashGamemodeLibrary.Util.Timer;
using UnityEngine;
using TeamManager = MashGamemodeLibrary.Player.Team.TeamManager;

namespace Clockhunt.Phase;

internal class ClockDeliveredPacket : INetSerializable
{
    public int ClockCount;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ClockCount);
    }
}

public class HuntPhase : GamePhase, ITimedPhase
{
    private static readonly WeightedRandomProvider<PlayerID> NightmarePlayerProvider = new(() =>
        {
            return NetworkPlayer.Players.Where(p => p.HasRig).Select(p => p.PlayerID).ToList();
        }
    );

    private static readonly SyncedVariable<Vector3> DeliveryPosition = new("deliveryposition", new Vector3Encoder(), Vector3.zero);

    private static readonly RemoteEvent<ClockDeliveredPacket> OnClockDeliveredEvent =
        new("Clockhunt_HuntPhase_OnClockDelivered", OnClockDelivered, CommonNetworkRoutes.HostToAll);

    private readonly RemoteEvent<DummySerializable> _teleportToSpawnEvent;

    public override string Name => "Hunt";
    public override float Duration => Clockhunt.Config.HuntPhaseDuration;

    protected override TimeMarker[] Markers => new[]
    {
        new TimeMarker(MarkerType.BeforeEnd, 60f, timer => Notifier.Send(new Notification()))
    };

    public HuntPhase()
    {
        _teleportToSpawnEvent =
            new RemoteEvent<DummySerializable>("Clockhunt_HuntPhase_TeleportToSpawn", OnTeleportToSpawnRequest,
                CommonNetworkRoutes.HostToAll);

        DeliveryPosition.OnValueChanged += value =>
        {
            if (!Clockhunt.IsStarted)
                return;

            if (value == Vector3.zero)
                return;

            // We want the marker to show up if we are debugging
#if !DEBUG
            if (NightmareManager.IsNightmare(PlayerIDManager.LocalID))
                return;
#endif

            MarkerManager.SetMarker(value);
        };
    }

    private void OnTeleportToSpawnRequest(DummySerializable _)
    {
        if (!NightmareManager.IsNightmare(PlayerIDManager.LocalID))
        {
            if (Clockhunt.Config.RuntimeSpawnPointsEnabled)
            {
                FusionPlayer.SetSpawnPoints(SpawnManager.GetSpawnPoints());
            }
            else
            {
                var spawns = GamemodeMarker.FilterMarkers();

                if (spawns.Count > 0)
                {
                    GamemodeHelper.SetSpawnPoints(spawns);
                    return;
                }
            }
        }

        if (Clockhunt.Config.TeleportToSpawn) GamemodeHelper.TeleportToSpawnPoint();
    }

    private void SetDeliveryPosition()
    {
        var spawnPoints = FusionPlayer.SpawnPoints;

        Vector3 position;
        if (spawnPoints.Count > 0)
            position = spawnPoints.GetRandom().position;
        else
            position = Clockhunt.Context.LocalPlayer.RigRefs.RigManager.checkpointPosition;

        DeliveryPosition.Value = position;
    }

    public override PhaseIdentifier GetNextPhase()
    {
        // Hide and seek
        if (Clockhunt.Config.GameType == GameType.HideAndSeek)
        {
            if (!HasReachedDuration()) return PhaseIdentifier.Empty();
            
            if (Clockhunt.Config.IsEscapePhaseEnabled)
            {
                return PhaseIdentifier.Of<EscapePhase>();
            }
          
            WinManager.Win<SurvivorTeam>();
            return PhaseIdentifier.Empty();
        }
        
        // Clockhunt
        if (HasReachedDuration())
        {
            WinManager.Win<NightmareTeam>();
            return PhaseIdentifier.Empty();
        }

        if (ClockManager.CountClockEntities() > 0) return PhaseIdentifier.Empty();

        if (!Clockhunt.Config.IsEscapePhaseEnabled)
        {
            WinManager.Win<SurvivorTeam>();
            return PhaseIdentifier.Empty();
        }

        return PhaseIdentifier.Of<EscapePhase>();
    }

    protected override void OnPhaseEnter()
    {
        Executor.RunIfHost(() =>
        {
            if (Clockhunt.Config.RuntimeSpawnPointsEnabled) 
                SpawnManager.SubmitSynced(Clockhunt.Config.RuntimeSpawnCount);

            var context = Clockhunt.Context;

#if DEBUG
            if (!Clockhunt.Config.DebugSkipNightmare) TeamManager.AssignRandom<NightmareTeam>(NightmarePlayerProvider);
#else
            TeamManager.AssignRandom<NightmareTeam>(NightmarePlayerProvider);
#endif
            _teleportToSpawnEvent.Call(new DummySerializable());

            EscapeManager.CollectEscapePoints();

            if (Clockhunt.Config.GameType == GameType.Clockhunt)
            {
                SetDeliveryPosition();
                ClockManager.RemoveUntilCount(Clockhunt.Config.HuntPhaseClockCount);
                context.ClockAudioPlayer.Start();
            }
        });
    }

// TODO: Make players pinged for the nightmare if time runs out
    protected override void OnUpdate()
    {
        FrogJumpscare.Update();

        Executor.RunIfHost(() =>
        {
            var clocks = EntityTagManager.GetAllWithTag<ObjectiveCollectable>(tag => tag.IsGrabbed);

            foreach (var networkEntity in from networkEntity in clocks
                     let marrowEntity = networkEntity.GetExtender<IMarrowEntityExtender>().MarrowEntity
                     let distance = Vector3.Distance(marrowEntity.transform.position, DeliveryPosition)
                     where distance <= Clockhunt.Config.DeliveryDistance
                     select networkEntity)
            {
                var count = EntityTagManager.CountEntitiesWithTag<ClockMarker>();

                NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo
                {
                    DespawnEffect = true,
                    EntityID = networkEntity.ID
                });

                // When the phase ends, there is a different message
                if (count > 1)
                    OnClockDeliveredEvent.Call(new ClockDeliveredPacket
                    {
                        ClockCount = count - 1
                    });
            }
        });
    }

    protected override void OnPhaseExit()
    {
        Executor.RunIfHost(() =>
        {
            DeliveryPosition.Value = Vector3.zero;

            if (ClockManager.CountClockEntities() == 0)
            {
                if (!Clockhunt.Config.IsEscapePhaseEnabled) WinManager.Win<SurvivorTeam>();
                return;
            }

            WinManager.Win<NightmareTeam>();
        });
    }

    public override void OnPlayerAction(PlayerID playerId, PlayerGameActions action, Handedness handedness)
    {
        NightmareManager.OnAction(playerId, action, handedness);
    }

    private static void OnClockDelivered(ClockDeliveredPacket packet)
    {
        Notifier.Send(new Notification
        {
            Title = "Clock Delivered",
            Message =
                $"A clock has been delivered! There are {Mathf.Max(0, ClockManager.CountClockEntities())} clocks remaining."
        });
    }
}