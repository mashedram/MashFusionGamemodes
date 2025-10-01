using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Game;
using Clockhunt.Nightmare.Implementations;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Phase;

namespace Clockhunt.Phase;

public class HuntPhase : GamePhase
{
    public override string Name => "Hunt";
    public override float Duration => ClockhuntConfig.HuntPhaseDuration;

    protected override void OnPhaseEnter()
    {
        if (!NetworkInfo.IsHost) 
            return;
        
        var context = Clockhunt.Context;
        
        ClockManager.RemoveUntilCount(ClockhuntConfig.HuntPhaseClockCount);

        var players = NetworkPlayer.Players.ToList();
        players.Shuffle();
        players.Take(ClockhuntConfig.NightmareCount).ForEach(context.NightmareManager.SetRandomNightmare);
        
        context.ClockAudioPlayer.StartPlaying();

        MultiplayerHooking.OnPlayerAction += OnPlayerAction;
    }

    protected override bool ShouldPhaseExit()
    {
        return ClockManager.CountClockEntities() == 0;
    }

    protected override void OnPhaseExit()
    {
        if (!NetworkInfo.IsHost)
            return;
        
        MultiplayerHooking.OnPlayerAction -= OnPlayerAction;
        
        if (ClockManager.CountClockEntities() == 0)
            return;
        
        WinStateManager.ForceWin(GameTeam.Nightmares);
    }

    public void OnPlayerAction(PlayerID playerId, PlayerActionType type, PlayerID otherPlayer)
    {
        if (!NetworkInfo.IsHost)
            return;
        
        if (type != PlayerActionType.DYING)
            return;
        
        WinStateManager.PlayerDied(playerId);
    }
}