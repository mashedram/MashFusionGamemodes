using BoneStrike.Tags;
using LabFusion.Player;
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

public class NightmareTeam : LogicTeam
{
    public override string Name => "Nightmare";

    public override void OnPhaseChanged(GamePhase phase)
    {
        // TODO : Assign nightmare
        
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            var playerLocked = Gamemode.TheHunt.Config.LockNightmare && phase is HidePhase;
            LocalControls.LockedMovement = playerLocked;
            LocalVision.Blind = playerLocked && Gamemode.TheHunt.Config.BlindNightmare;
        });
    }

    protected override void OnAssigned()
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            Owner.AddComponents(new PlayerHandTimerTag());
            Owner.RemoveComponent<LimitedRespawnComponent>();

            // TODO: Assign per nightmare
            PlayerStatManager.SetStats(new PlayerStats
            {
                Agility = 1.4f,
                LowerStrength = 3f,
                UpperStrength = 3f,
                Speed = 1.5f,
                Vitality = 10f
            });

            Notifier.Send(new Notification
            {
                Title = "The Nightmare",
                Message = $"You know what to do.",
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