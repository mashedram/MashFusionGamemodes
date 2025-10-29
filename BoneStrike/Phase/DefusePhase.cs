using BoneStrike.Tags;
using BoneStrike.Teams;
using LabFusion.Entities;
using LabFusion.RPC;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Spectating;
using MashGamemodeLibrary.Player.Team;
using MashGamemodeLibrary.Util;
using MashGamemodeLibrary.Util.Timer;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

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
        
        Executor.RunIfHost(() =>
        {
            BoneStrike.Context.BombAudioPlayer.Start();
        });
    }
}