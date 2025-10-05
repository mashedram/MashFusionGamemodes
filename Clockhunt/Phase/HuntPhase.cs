using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Entities.Tags;
using Clockhunt.Game;
using Clockhunt.Nightmare;
using Clockhunt.Nightmare.Implementations;
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
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Phase;
using UnityEngine;

namespace Clockhunt.Phase;

public class HuntPhase : GamePhase
{
    public override string Name => "Hunt";
    public override float Duration => ClockhuntConfig.HuntPhaseDuration;
    
    private readonly Vector3SyncedVariable _deliveryPosition;

    public HuntPhase()
    {
        _deliveryPosition = new Vector3SyncedVariable("deliveryposition", Vector3.zero);
        _deliveryPosition.OnValueChanged += value =>
        {
            if (!IsActive)
                return;
            
            MarkerManager.SetMarker(value);
        };
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
        
        _deliveryPosition.Value = position;
    }

    protected override void OnPhaseEnter()
    {
        
        Executor.RunIfHost(() =>
        {
            var context = Clockhunt.Context;
            
            SetDeliveryPosition();
            
            var players = NetworkPlayer.Players.ToList();
            players.Shuffle();
            players.Take(ClockhuntConfig.NightmareCount).ForEach(NightmareManager.SetRandomNightmare);
            
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
                     let distance = Vector3.Distance(marrowEntity.transform.position, _deliveryPosition) 
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