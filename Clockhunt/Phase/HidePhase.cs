using Clockhunt.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace Clockhunt.Phase;

public class HidePhase : GamePhase
{
    public override string Name => "Hide";
    public override float Duration => 20f;

    protected override void OnPhaseEnter()
    {
        var localPlayer = LocalPlayer.GetNetworkPlayer();
        if (localPlayer == null)
            return;
        
        ClockManager.SpawnEntityForPlayer(localPlayer);
    }

    protected override void OnUpdate()
    {
        
    }
}