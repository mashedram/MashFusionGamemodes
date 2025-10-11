using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Game;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace Clockhunt.Phase;

public class HidePhase : GamePhase
{
    public override string Name => "Hide";
    public override float Duration => ClockhuntConfig.HidePhaseDuration;

    protected override void OnPhaseEnter()
    {
        WinStateManager.SetLocalTeam(GameTeam.Survivors);
        
        PlayerStatManager.SetStats(ClockhuntConfig.DefaultStats);
        
        Executor.RunIfHost(() =>
        {
            NetworkPlayer.Players.ForEach(player =>
            {
                if (!player.HasRig) return;
                
                // TODO: Make this spawn a new one once the old one was placed
                for (var i = 0; i < ClockhuntConfig.ClocksPerPlayer; i++)
                    ClockManager.SpawnEntityForPlayer(player);
            });
        });

        if (ClockhuntConfig.RuntimeSpawnPointsEnabled)
        {
            SpawnManager.Reset();
        }
    }

    protected override void OnUpdate()
    {
        if (ClockhuntConfig.RuntimeSpawnPointsEnabled)
        {
            SpawnManager.Update(Time.deltaTime);
        }
    }
}