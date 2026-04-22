using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Extensions;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.CommonComponents;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Loadout;
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

            var weaponCrates = Gamemode.TheHunt.Config.WeaponItemCrates;
            var weaponBarcode = weaponCrates.Count > 0 ? new Barcode(weaponCrates.GetRandom()) : null;
            var flashlightBarcode = Gamemode.TheHunt.Config.LightItemCrate;
            var flashlightCrate = string.IsNullOrEmpty(flashlightBarcode) ? null : new Barcode(flashlightBarcode);
            new Loadout()
                .SetSlotBarcode(SlotType.RightBack, weaponBarcode)
                .SetSlotBarcode(SlotType.LeftBack, flashlightCrate)
                .Assign();
            
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
                Speed = Gamemode.TheHunt.Config.HiderSpeed,
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