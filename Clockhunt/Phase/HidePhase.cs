using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Game;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
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
        var context = Clockhunt.Context;
        WinStateManager.SetLocalTeam(GameTeam.Survivors);
        
        PlayerStatManager.SetStats(ClockhuntConfig.DefaultStats);
        
        if (!NetworkInfo.IsHost)
            return;
        
        NetworkPlayer.Players.ForEach(player =>
        {
            // TODO: Make this spawn a new one once the old one was placed
            // TODO: Make this spawn a clock in an inventory slot
            for (var i = 0; i < ClockhuntConfig.ClocksPerPlayer; i++)
                ClockManager.SpawnEntityForPlayer(player);
        });
    }
}