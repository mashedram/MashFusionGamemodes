using BoneStrike.Tags;
using BoneStrike.Teams;
using LabFusion.Extensions;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.CommonComponents;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Team;
using MashGamemodeLibrary.Util.Timer;

namespace BoneStrike.Phase;

public class DefusePhase : GamePhase
{
    public override string Name => "Defuse Phase";
    public override float Duration => BoneStrike.Config.DefuseDuration;

    protected override TimeMarker[] Markers => new[]
    {
        CommonTimeMarkerEvents.TimeRemaining(10f),
        new TimeMarker(MarkerType.BeforeEnd, 30f, _ =>
        {
            if (!BoneStrike.Config.MarkBombNearEnd)
                return;

            foreach (var bombMarker in BombMarker.Query)
            {
                bombMarker.NetworkEntity?.AddComponent(new OvertimeMarker());
            }
        }),
        CommonTimeMarkerEvents.TimeRemaining(60f)
    };

    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration()) return PhaseIdentifier.Empty();

        BoneStrike.ExplodeAllBombs();
        WinManager.Win<TerroristTeam>();

        return PhaseIdentifier.Empty();
    }

    protected override void OnPhaseEnter()
    {
        Notifier.Send(new Notification
        {
            Title = "Game Start!",
            Message = LogicTeamManager.IsLocalTeam<TerroristTeam>() ? "Defend the bomb!" : "Defuse the bomb!",
            ShowPopup = true,
            SaveToMenu = false,
            PopupLength = 4f,
            Type = NotificationType.INFORMATION
        });

        Executor.RunCheckedInFuture(DropClock, TimeSpan.FromSeconds(5));

        foreach (var entry in PlayerHandTimerTag.Query)
        {
            entry.SetTarget(() => BombMarker.Query
                .Where(p => p.MarrowEntity != null)
                .Select(p => p.MarrowEntity!.transform.position)
            );
        }
        Executor.RunIfHost(() =>
        {
            BoneStrike.Context.BombAudioPlayer.Start();
            BoneStrike.Context.DefusePhaseStartAudioPlayer.PlayRandom();
            
            PlayerDataManager.ModifyAll<HideEnemyNametagsRule>(rule => rule.IsEnabled = true);
        });
    }

    private static void DropClock()
    {
        if (!LogicTeamManager.IsLocalTeam<TerroristTeam>())
            return;

        foreach (var gripWithHand in PlayerGrabManager.GetLocalHandsHoldingTag<BombMarker>())
        {
            gripWithHand.Hand.TryDetach();
        }
    }
}