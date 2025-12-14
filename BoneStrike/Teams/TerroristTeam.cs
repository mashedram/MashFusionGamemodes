using BoneStrike.Phase;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Team;

namespace BoneStrike.Teams;

public class TerroristTeam : Team
{
    public override string Name => "Terrorists";
    public virtual uint Capacity => UInt32.MaxValue;
    public virtual uint Weight => 1;

    public override void OnPhaseChanged(GamePhase phase)
    {
        Executor.RunIfHost(() =>
        {
            Owner.ToggleTag(phase is DefusePhase, () => new LimitedRespawnTag(BoneStrike.Config.MaxRespawns));
        });
    }

    protected override void OnAssigned()
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            Notifier.Send(new Notification
            {
                Title = "Terrorists",
                Message = $"Hide and defend the bomb. You have {BoneStrike.Config.MaxRespawns} lives and can skip ahead by holding the bomb and tapping the menu key.",
                PopupLength = 10f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        });
    }
}