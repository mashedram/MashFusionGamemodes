using BoneStrike.Phase;
using BoneStrike.Tags;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.CommonComponents;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;

namespace BoneStrike.Teams;

public class WildCardTeamMembership : LogicTeam
{

    public override string Name => "Wildcard";
    
    public override void OnPhaseChanged(GamePhase phase)
    {
        Executor.RunIfHost(() =>
        {
            Owner.ToggleComponent(phase is DefusePhase, () => new LimitedRespawn(0));
        });
    }

    protected override void OnAssigned()
    {
        Owner.AddComponent(new KillEffect());

        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            Owner.AddComponent(new PlayerHandTimerTag());

            AvatarStatManager.SetStats(new AvatarStats
            {
                Agility = 1.2f,
                LowerStrength = 1.2f,
                UpperStrength = 1.2f,
                Speed = BoneStrike.Config.MovementSpeedMultiplier,
                Vitality = 1f
            }.MultiplyHealth(BoneStrike.Config.AttackerHealthMultiplier));

            Notifier.Send(new Notification
            {
                Title = "Wildcard",
                Message = "Be the last one standing to take victory for yourself.",
                PopupLength = 10f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        });
    }
}