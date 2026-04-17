using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Components;
using TheHunt.Phase;

namespace TheHunt.Teams;

public class HiderTeam : LogicTeam
{
    public override string Name => "Hiders";
    private string? _originalAvatarBarcode;

    public override void OnPhaseChanged(GamePhase phase)
    {
        Executor.RunIfHost(() =>
        {
            Owner.ToggleComponent(phase is not HidePhase, () => new LimitedRespawnComponent(0));
        });
        
        // Run for all
        if (LogicTeamManager.IsLocalTeam<NightmareTeam>())
            Owner.ToggleComponent(phase is FinallyPhase, () => new HiderFinallyMarker());
    }

    protected override void OnAssigned()
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            Owner.AddComponent(new PlayerHandTimerComponent());
            
            if (Gamemode.TheHunt.Config.LockHiderAvatars)
            {
                _originalAvatarBarcode = LocalAvatar.AvatarBarcode;
                LocalAvatar.AvatarOverride = _originalAvatarBarcode;
            }

            AvatarStatManager.SetStats(new AvatarStats
            {
                Agility = 1.35f,
                LowerStrength = 1.1f,
                UpperStrength = 1.1f,
                Speed = 1.30f,
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

    protected override void OnRemoved()
    {
        Executor.RunIfMe(Owner.PlayerID, () =>
        {
            if (_originalAvatarBarcode != null)
                LocalAvatar.SwapAvatarCrate(_originalAvatarBarcode);
        });
    }
}