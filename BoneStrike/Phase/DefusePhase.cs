using BoneStrike.Tags;
using BoneStrike.Teams;
using LabFusion.Extensions;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Util.Timer;
using MelonLoader;
using TeamManager = MashGamemodeLibrary.Player.Team.TeamManager;

namespace BoneStrike.Phase;

public class DefusePhase : GamePhase
{
    public override string Name => "Defuse Phase";
    public override float Duration => BoneStrike.Config.DefuseDuration;

    protected override TimeMarker[] Markers => new[]
    {
        CommonTimeMarkerEvents.TimeRemaining(10f),
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
            Message = TeamManager.IsLocalTeam<TerroristTeam>() ? "Defend the bomb!" : "Defuse the bomb!",
            ShowPopup = true,
            SaveToMenu = false,
            PopupLength = 4f,
            Type = NotificationType.INFORMATION
        });

        Executor.RunCheckedInFuture(DropClock, TimeSpan.FromSeconds(5));

        var bomb = BombMarker.Query.FirstOrDefault();
        if (bomb != null)
        {
            foreach (var entry in PlayerHandTimerTag.Query)
            {
                if (bomb.MarrowEntity == null)
                    continue;
                
                entry.Target = bomb.MarrowEntity.gameObject;
            }
        }
        else
        {
            MelonLogger.Msg("Failed to find a bomb. This is a bug.");
        }

        Executor.RunIfHost(() =>
        {
            BoneStrike.Context.BombAudioPlayer.Start();
            BoneStrike.Context.DefusePhaseStartAudioPlayer.PlayRandom();
        });
    }

    private static void DropClock()
    {
        if (!TeamManager.IsLocalTeam<TerroristTeam>())
            return;
        
        foreach (var gripWithHand in PlayerGrabManager.GetLocalHandsHoldingTag<BombMarker>())
        {
            gripWithHand.Hand.TryDetach();
        }
    }
}