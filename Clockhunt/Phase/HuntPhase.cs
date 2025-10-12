using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Entities.Tags;
using Clockhunt.Game;
using Clockhunt.Joke;
using Clockhunt.Nightmare;
using Clockhunt.Nightmare.Implementations;
using Clockhunt.Vision;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Integration;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace Clockhunt.Phase;

internal class ClockDeliveredPacket : INetSerializable
{
    public int ClockCount;
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ClockCount);
    }
}

public class HuntPhase : GamePhase
{
    public override string Name => "Hunt";
    public override float Duration => ClockhuntConfig.HuntPhaseDuration;
    private static readonly Vector3SyncedVariable DeliveryPosition = new("deliveryposition", Vector3.zero);
    private readonly RemoteEvent<DummySerializable> _teleportToSpawnEvent;
    private static readonly RemoteEvent<ClockDeliveredPacket> OnClockDeliveredEvent = new("Clockhunt_HuntPhase_OnClockDelivered", OnClockDelivered, true);

    public HuntPhase()
    {
        _teleportToSpawnEvent = new RemoteEvent<DummySerializable>("Clockhunt_HuntPhase_TeleportToSpawn", OnTeleportToSpawnRequest, true);
        
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
        // if (!NightmareManager.IsNightmare(PlayerIDManager.LocalID))
        // {
            if (ClockhuntConfig.RuntimeSpawnPointsEnabled)
            {
                FusionPlayer.SetSpawnPoints(SpawnManager.GetSpawnPoints());
            }
            else
            {
                var spawns = GamemodeMarker.FilterMarkers(null);

                if (spawns.Count > 0)
                {
                    GamemodeHelper.SetSpawnPoints(spawns);
                    return;
                }
            }
        // }
        
        if (ClockhuntConfig.TeleportToSpawn)
        {
            GamemodeHelper.TeleportToSpawnPoint();
        }
    }
    
    private void SetDeliveryPosition()
    {
        var spawnPoints = FusionPlayer.SpawnPoints;

        Vector3 position;
        if (spawnPoints.Count > 0)
        {
            position = spawnPoints.GetRandom().position;
        }
        else
        {
            position = Clockhunt.Context.LocalPlayer.RigRefs.RigManager.checkpointPosition;
        }
        
        DeliveryPosition.Value = position;
    }

    protected override void OnPhaseEnter()
    {
        Executor.RunIfHost(() =>
        {
            if (ClockhuntConfig.RuntimeSpawnPointsEnabled)
            {
                SpawnManager.SubmitSynced(ClockhuntConfig.RuntimeSpawnCount);
            }
            
            var context = Clockhunt.Context;

            NightmareManager.SetRandomNightmare();
            
            _teleportToSpawnEvent.Call(new DummySerializable());
            
            SetDeliveryPosition();
            
            EscapeManager.CollectEscapePoints();
            
            ClockManager.RemoveUntilCount(ClockhuntConfig.HuntPhaseClockCount);
        
            context.ClockAudioPlayer.StartPlaying();
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
                     where distance <= ClockhuntConfig.DeliveryDistance 
                     select networkEntity)
            {
                var count = EntityTagManager.CountEntitiesWithTag<ClockMarker>();
                
                NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
                {
                    DespawnEffect = true,
                    EntityID = networkEntity.ID
                });

                // When the phase ends, there is a different message
                if (count > 1)
                {
                    OnClockDeliveredEvent.Call(new ClockDeliveredPacket
                    {
                        ClockCount = count - 1
                    });
                }
            }
        });
    }

    protected override bool ShouldPhaseExit()
    {
        return ClockManager.CountClockEntities() == 0;
    }

    protected override void OnPhaseExit()
    {
        DeliveryPosition.Value = Vector3.zero;
        
        Executor.RunIfHost(() =>
        {
            if (ClockManager.CountClockEntities() == 0)
            {
                if (!ClockhuntConfig.IsEscapePhaseEnabled)
                {
                    WinStateManager.ForceWin(GameTeam.Survivors);    
                }
                return;
            }
        
            WinStateManager.ForceWin(GameTeam.Nightmares);
        });
    }

    public override void OnPlayerAction(PlayerID playerId, PhaseAction action, Handedness handedness)
    {
        NightmareManager.OnAction(playerId, action, handedness);
        if (action != PhaseAction.Death)
            return;
        
        WinStateManager.PlayerDied(playerId);
    }

    private static void OnClockDelivered(ClockDeliveredPacket packet)
    {
        Notifier.Send(new Notification
        {
            Title = "Clock Delivered",
            Message = $"A clock has been delivered! There are {Mathf.Max(0, ClockManager.CountClockEntities())} clocks remaining.",
        });
    }
}