using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Game;
using Clockhunt.Nightmare;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;

namespace Clockhunt.Phase;

public class EscapePhase : GamePhase, ITimedPhase
{
    public override string Name => "Escape";
    public float Duration => 300f;
    
    public override PhaseIdentifier GetNextPhase()
    {
        return PhaseIdentifier.Empty();
    }

    protected override void OnPhaseEnter()
    {
        Executor.RunIfHost(() =>
        {
            EscapeManager.ActivateRandomEscapePoint();
            PlayerControllerManager.OnAll<ClockhuntPlayerController>(controller =>
            {
                controller.SetLives(0);
            });
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

    protected override void OnUpdate()
    {
        EscapeManager.Update(Time.deltaTime);
    }

    public override void OnPlayerAction(PlayerID playerId, PlayerGameActions action, Handedness handedness)
    {
        NightmareManager.OnAction(playerId, action, handedness);
    }
}