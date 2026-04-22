using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.CommonComponents;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Components;
using TheHunt.Nightmare;
using TheHunt.Phase;

namespace TheHunt.Teams;

public class NightmareTeam : LogicTeam
{
    public override string Name => "Nightmare";

    public override void OnPhaseChanged(GamePhase phase)
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            var playerLocked = Gamemode.TheHunt.Config.LockNightmare && phase is HidePhase;
            LocalControls.DisableInventory = playerLocked;
            LocalControls.LockedMovement = playerLocked;
            LocalVision.Blind = playerLocked && Gamemode.TheHunt.Config.BlindNightmare;
            
            PlayerGrabManager.SetOverwrite(Name, CanGrab);
        });
    }

    protected override void OnAssigned()
    {
        Executor.RunIfHost(() =>
        {
            Owner.AddComponents(NightmareComponent.AsRandomNightmare());
            Owner.RemoveComponent<LimitedRespawnComponent>();
        });
        
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            Owner.AddComponent(new PlayerHandTimerComponent());
            LocalHealth.MortalityOverride = false;
            
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
            
            PlayerGrabManager.SetOverwrite(Name, null);
        });
    }
    
    public bool CanGrab(GrabData grabData)
    {
        return !grabData.IsHoldingItem(out var item) || NetworkPlayerManager.TryGetPlayer(item.MarrowEntity, out _);
    }
}