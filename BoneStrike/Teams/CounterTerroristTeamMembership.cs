using BoneStrike.Phase;
using BoneStrike.Tags;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using Team = MashGamemodeLibrary.Player.Team.Team;

namespace BoneStrike.Teams;

public class CounterTerroristTeam : Team
{
    public override string Name => "Counter Terrorists";

    public override void OnPhaseChanged(GamePhase phase)
    {
        Executor.RunIfHost(() =>
        {
            Owner.ToggleTag(phase is DefusePhase, () => new LimitedRespawnComponent(BoneStrike.Config.MaxRespawns));
        });

        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            var isLocked = phase is PlantPhase;
            LocalControls.LockedMovement = isLocked;
        });
    }

    protected override void OnAssigned()
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            Owner.AddTag(new PlayerHandTimerTag());
            
            Notifier.Send(new Notification
            {
                Title = "Counter Terrorists",
                Message = $"Once the bomb has been planted, disarm it by holding it. You have {BoneStrike.Config.MaxRespawns} lives.",
                PopupLength = 10f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        });
    }

    protected override void OnRemoved()
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            LocalControls.LockedMovement = false;
        });
    }
}