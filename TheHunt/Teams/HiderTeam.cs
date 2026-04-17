using BoneStrike.Tags;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Phase;

namespace TheHunt.Teams;

public class HiderTeam : LogicTeam
{
    public override string Name => "Hiders";

    public override void OnPhaseChanged(GamePhase phase)
    {
        Executor.RunIfHost(() =>
        {
            Owner.ToggleComponent(phase is not HidePhase, () => new LimitedRespawnComponent(0));
        });
    }

    protected override void OnAssigned()
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            Owner.AddComponents(new PlayerHandTimerTag());

            PlayerStatManager.SetStats(new PlayerStats
            {
                Agility = 1.2f,
                LowerStrength = 1.1f,
                UpperStrength = 1.1f,
                Speed = 1.32f,
                Vitality = 1f
            });

            Notifier.Send(new Notification
            {
                Title = "Hiders",
                Message = $"Get away.",
                PopupLength = 10f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        });
    }
}