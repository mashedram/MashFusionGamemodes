using BoneStrike.Phase;
using BoneStrike.Tags;
using LabFusion.Menu;
using LabFusion.Player;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;

namespace BoneStrike.Teams;

public class TerroristTeam : LogicTeam
{
    public override string Name => "Terrorists";
    public override Texture Icon { get; } = MenuResources.GetLogoIcon("LavaGang");

    public override void OnPhaseChanged(GamePhase phase)
    {
        Executor.RunIfHost(() =>
        {
            Owner.ToggleTag(phase is DefusePhase, () => new LimitedRespawnComponent(0));
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
            }.MultiplyHealth(BoneStrike.Config.DefenderHealthMultiplier));

            Notifier.Send(new Notification
            {
                Title = "Terrorists",
                Message =
                    $"Hide and defend the bomb. Teleport the bomb to you by tapping the menu button if you lose it.",
                PopupLength = 10f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        });
    }
}