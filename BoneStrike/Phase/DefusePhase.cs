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
using MashGamemodeLibrary.Player.Team;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Util;

namespace BoneStrike.Phase;

public class DefusePhase : GamePhase
{
    private const string ExplosionBarcode = "BaBaCorp.MiscExplosiveDevices.Spawnable.ExplosionMedBigDamge";

    private static readonly RemoteEvent<DummySerializable> OnMinuteLeft = new("OneDefuseMinuteLeft", _ =>
    {
        Notifier.Send(new Notification
        {
            Title = "One Minute Left",
            PopupLength = 5f,
            ShowPopup = true,
            SaveToMenu = false,
            Type = NotificationType.WARNING
        });
    });
    
    public override string Name => "Defuse Phase";
    public override float Duration => BoneStrike.Config.DefuseDuration;

    protected override TimeMarker[] Markers => new[]
    {
        new TimeMarker(MarkerType.BeforeEnd, 60f, _ => OnMinuteLeft.Call(new DummySerializable()))
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
        Executor.RunIfHost(() =>
        {
            BoneStrike.Context.BombAudioPlayer.Start();
        });
    }
}