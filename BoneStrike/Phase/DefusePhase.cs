using BoneStrike.Tags;
using BoneStrike.Teams;
using LabFusion.Entities;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
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

        var bomb = EntityTagManager.GetAllWithTag<BombMarker>().FirstOrDefault();
        if (bomb != null)
        {
            var marrow = bomb.GetExtender<IMarrowEntityExtender>();
            if (marrow != null)
            {
                foreach (var (_, tag) in EntityTagManager.GetAllTags<PlayerHandTimerTag>())
                {
                    tag._target = marrow.MarrowEntity.gameObject;
                }
            }
        }

        Executor.RunIfHost(() =>
        {
            BoneStrike.Context.BombAudioPlayer.Start();
        });
    }
}