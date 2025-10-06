using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Entities.Tags;
using Clockhunt.Game;
using Clockhunt.Nightmare;
using Clockhunt.Nightmare.Implementations;
using Clockhunt.Vision;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Integration;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.SDK.Gamemodes;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace Clockhunt.Phase;

public class HuntPhase : GamePhase
{
    public override string Name => "Hunt";
    public override float Duration => ClockhuntConfig.HuntPhaseDuration;
    private static readonly Vector3SyncedVariable DeliveryPosition = new("deliveryposition", Vector3.zero);
    private readonly RemoteEvent<DummySerializable> _teleportToSpawnEvent;

    public HuntPhase()
    {
        _teleportToSpawnEvent = new RemoteEvent<DummySerializable>("Clockhunt_HuntPhase_TeleportToSpawn", OnTeleportToSpawnRequest, true);
        
        DeliveryPosition.OnValueChanged += value =>
        {
            if (!Clockhunt.IsStarted) 
                return;
            
            if (value == Vector3.zero)
                return;
            
            if (NightmareManager.IsNightmare(PlayerIDManager.LocalID))
                return;
            
            MarkerManager.SetMarker(value);
        };
    }

    private void OnTeleportToSpawnRequest(DummySerializable _)
    {
        if (!NightmareManager.IsNightmare(PlayerIDManager.LocalID))
        {
            GamemodeHelper.SetSpawnPoints(GamemodeMarker.FilterMarkers(null));
        }
        
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
            var context = Clockhunt.Context;
            
            var players = NetworkPlayer.Players.ToList();
            players.Shuffle();
            players.Take(ClockhuntConfig.NightmareCount).ForEach(NightmareManager.SetRandomNightmare);
            
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
        Executor.RunIfHost(() =>
        {
            foreach (var networkEntity in from networkEntity in EntityTagManager.GetAllWithTag<ObjectiveCollectable>(tag => tag.IsGrabbed) 
                     let marrowEntity = networkEntity.GetExtender<IMarrowEntityExtender>().MarrowEntity 
                     let distance = Vector3.Distance(marrowEntity.transform.position, DeliveryPosition) 
                     where distance <= ClockhuntConfig.DeliveryDistance 
                     select networkEntity)
            {
                NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
                {
                    DespawnEffect = true,
                    EntityID = networkEntity.ID
                });
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
            MultiplayerHooking.OnPlayerAction -= OnPlayerAction;

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

    public override void OnPlayerAction(PlayerID playerId, PlayerActionType type, PlayerID otherPlayer)
    {
        if (type != PlayerActionType.DEATH)
            return;
        
        WinStateManager.PlayerDied(playerId);
    }
}