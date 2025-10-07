using Clockhunt.Config;
using Clockhunt.Entities;
using Clockhunt.Game;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using UnityEngine;

namespace Clockhunt.Phase;

public class EscapePhase : GamePhase
{
    public override string Name => "Escape";
    public override float Duration => 300f;
    
    protected override void OnPhaseEnter()
    {
        Executor.RunIfHost(() =>
        {
            EscapeManager.ActivateRandomEscapePoint();
            WinStateManager.OverwriteLives(0, false);
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

    protected override bool PhaseEnterPredicate()
    {
        return ClockhuntConfig.IsEscapePhaseEnabled && ClockManager.CountClockEntities() <= 0;
    }
}