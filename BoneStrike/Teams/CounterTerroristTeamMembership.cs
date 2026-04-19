using BoneStrike.Phase;
using BoneStrike.Tags;
using LabFusion.Menu;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;

namespace BoneStrike.Teams;

public class CounterTerroristTeam : LogicTeam
{
    public override string Name => "Counter Terrorists";
    public override Texture Icon { get; } = MenuResources.GetLogoIcon("Sabrelake");

    public override void OnPhaseChanged(GamePhase phase)
    {
        Executor.RunIfHost(() =>
        {
            Owner.ToggleComponent(phase is DefusePhase, () => new LimitedRespawnComponent(0));
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
            Owner.AddComponent(new PlayerHandTimerTag());

            AvatarStatManager.SetStats(new AvatarStats
            {
                Agility = 1.2f,
                LowerStrength = 1.2f,
                UpperStrength = 1.2f,
                Speed = 1.35f,
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