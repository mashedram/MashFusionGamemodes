using LabFusion.UI.Popups;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Team;

namespace BoneStrike.Teams;

public class TerroristTeam : Team
{
    public override string Name => "Terrorists";
    public override uint Capacity => UInt32.MaxValue;
    public override uint Weight => 1;

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