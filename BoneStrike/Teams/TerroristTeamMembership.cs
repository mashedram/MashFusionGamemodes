using BoneStrike.Phase;
using BoneStrike.Tags;
using LabFusion.Menu;
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

public class TerroristTeam : LogicTeam
{
    public override string Name => "Sabrelake";
    public override Texture Icon { get; } = MenuResources.GetLogoIcon("Sabrelake");

    public override void OnPhaseChanged(GamePhase phase)
    {
        Executor.RunIfHost(() =>
        {
            Owner.ToggleComponent(phase is DefusePhase, () => new LimitedRespawnComponent(0));
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
            }.MultiplyHealth(BoneStrike.Config.DefenderHealthMultiplier));

            Notifier.Send(new Notification
            {
                Title = "Sabrelake",
                Message =
                    "Hide and defend the alarm clock and purge the simulation.",
                PopupLength = 10f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        });
    }
}