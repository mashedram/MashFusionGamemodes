using BoneStrike.Phase;
using BoneStrike.Tags;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Stats;
using Team = MashGamemodeLibrary.Player.Team.Team;
using TeamManager = MashGamemodeLibrary.Player.Team.TeamManager;

namespace BoneStrike.Teams;

public class CounterTerroristTeam : Team
{
    public override string Name => "Counter Terrorists";

    public override void OnPhaseChanged(GamePhase phase)
    {
        Executor.RunIfHost(() =>
        {
            Owner.ToggleTag(phase is DefusePhase, () => new LimitedRespawnComponent(0));
        });
        
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            var isLocked = phase is PlantPhase;
            LocalControls.LockedMovement = isLocked;
            LocalVision.Blind = phase is PlantPhase && BoneStrike.Config.BlindAttackersDuringPlanting;
        });
    }

    protected override void OnAssigned()
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            Owner.AddTag(new PlayerHandTimerTag());
            
            PlayerStatManager.SetStats(new PlayerStats
            {
                Agility = 1.2f,
                LowerStrength = 1.2f,
                UpperStrength = 1.2f,
                Speed = 1.5f,
                Vitality = 1f
            }.MultiplyHealth(BoneStrike.Config.AttackerHealthMultiplier));
            
            Notifier.Send(new Notification
            {
                Title = "Counter Terrorists",
                Message = $"Once the bomb has been planted, disarm it by holding it.",
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