using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Game;
using Clockhunt.Game.Player;
using Clockhunt.Game.Teams;
using Clockhunt.Nightmare;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;

namespace Clockhunt.Phase;

public class EscapePhase : GamePhase, ITimedPhase
{
    public override string Name => "Escape";
    public override float Duration => Clockhunt.Config.EscapePhaseDuration;
    
    public override PhaseIdentifier GetNextPhase()
    {
        return PhaseIdentifier.Empty();
    }

    protected override void OnPhaseEnter()
    {
        Executor.RunIfHost(() =>
        {
            var escapePoint = EscapeManager.EscapePoint ?? Vector3.zero;
            PlayerEscapeTag.EscapePosition.Value = escapePoint;
            PlayerTagManager.OnAll<LimitedRespawnTag>(controller =>
            {
                controller.SetRespawns(0);
            });
            
            var context = Clockhunt.Context;
            var name = context.EscapeAudioPlayer.GetRandomAudioName();
            context.EscapeAudioPlayer.Play(name, escapePoint);
        });

        Notifier.Send(new Notification
        {
            Title = "All clocks delivered",
            Message = "Follow the alarm to the escape point! This is your one chance.",
            SaveToMenu = false,
            PopupLength = 5f,
            ShowPopup = true,
            Type = NotificationType.SUCCESS
        });
    }

    public override void OnPlayerAction(PlayerID playerId, PlayerGameActions action, Handedness handedness)
    {
        NightmareManager.OnAction(playerId, action, handedness);
    }
}