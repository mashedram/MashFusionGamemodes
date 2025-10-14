using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Game;
using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Stats;
using UnityEngine;

namespace Clockhunt.Phase;

public class HidePhase : GamePhase, ITimedPhase
{
    public override string Name => "Hide";
    public float Duration => ClockhuntConfig.HidePhaseDuration;

    public override PhaseIdentifier GetNextPhase()
    {
        if (ElapsedTime < Duration) return PhaseIdentifier.Empty();
        
        return PhaseIdentifier.Of<HuntPhase>();
    }

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

        if (ClockhuntConfig.RuntimeSpawnPointsEnabled) SpawnManager.Reset();
    }

    protected override void OnUpdate()
    {
        if (ClockhuntConfig.RuntimeSpawnPointsEnabled) SpawnManager.Update(Time.deltaTime);
    }
}