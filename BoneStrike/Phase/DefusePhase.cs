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

namespace BoneStrike.Phase;

public class DefusePhase : GamePhase
{
    private const string ExplosionBarcode = "BaBaCorp.MiscExplosiveDevices.Spawnable.ExplosionMedBigDamge";
    
    public override string Name => "Defuse Phase";
    public override float Duration => BoneStrike.Config.DefuseDuration;

    protected override TimeMarker[] Markers => new[]
    {
        CommonTimeMarkerEvents.TimeRemaining(10f),
        CommonTimeMarkerEvents.TimeRemaining(60f)
    };

    private static bool HasDefusers()
    {
        return NetworkPlayer.Players.Any(player => 
            player.HasRig && player.PlayerID.IsTeam<CounterTerroristTeam>() && !player.PlayerID.IsSpectating()
        );
    }
    
    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration() && HasDefusers()) return PhaseIdentifier.Empty();
        
        foreach (var networkEntity in EntityTagManager.GetAllWithTag<BombMarker>())
        {
            var marrow = networkEntity.GetExtender<IMarrowEntityExtender>();
            if (marrow == null) continue;

            var position = marrow.MarrowEntity.transform.position;
            GameAssetSpawner.SpawnNetworkAsset(ExplosionBarcode, position);
        }
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