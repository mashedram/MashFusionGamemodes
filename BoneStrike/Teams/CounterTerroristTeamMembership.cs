using BoneStrike.Phase;
using BoneStrike.Tags;
using LabFusion.Menu;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.CommonComponents;
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
    public override string Name => "LavaGang";
    public override Texture Icon { get; } = MenuResources.GetLogoIcon("LavaGang");

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
        Owner.AddComponent(new KillEffectComponent());
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
                Title = "LavaGang",
                Message = $"Stop Sabrelake from ending the simulation by disabling the alarm!",
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