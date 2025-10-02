using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Entities.Tags;
using Clockhunt.Game;
using Clockhunt.Nightmare;
using Clockhunt.Nightmare.Implementations;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using UnityEngine;

namespace Clockhunt.Phase;

public class HuntPhase : GamePhase
{
    public override string Name => "Hunt";
    public override float Duration => ClockhuntConfig.HuntPhaseDuration;
    
    private static Vector3 _deliveryPosition = new(0, 1, 0);

    protected override void OnPhaseEnter()
    {
        Executor.RunIfHost(() =>
        {
            var context = Clockhunt.Context;
        
            ClockManager.RemoveUntilCount(ClockhuntConfig.HuntPhaseClockCount);

            var players = NetworkPlayer.Players.ToList();
            players.Shuffle();
            players.Take(ClockhuntConfig.NightmareCount).ForEach(NightmareManager.SetRandomNightmare);
        
            context.ClockAudioPlayer.StartPlaying();

            MultiplayerHooking.OnPlayerAction += OnPlayerAction;
            
            EscapeManager.CollectEscapePoints();
        });
    }

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
        
            ClockManager.ClearClocks();
            WinStateManager.ForceWin(GameTeam.Nightmares);
        });
    }
    
    public static void SetDeliveryPosition(Vector3 position)
    {
        _deliveryPosition = position;
    }

    private static void OnPlayerAction(PlayerID playerId, PlayerActionType type, PlayerID otherPlayer)
    {
        if (type != PlayerActionType.DYING)
            return;
        
        WinStateManager.PlayerDied(playerId);
    }
}