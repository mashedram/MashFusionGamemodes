using BoneStrike.Tags;
using BoneStrike.Teams;
using HarmonyLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Spawning;
using MashGamemodeLibrary.Player.Team;
using MashGamemodeLibrary.Util.Timer;
using MelonLoader;
using ExecutionContext = MashGamemodeLibrary.Execution.ExecutionContext;

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

        var bomb = EntityTagManager.GetAllWithTag<BombMarker>().FirstOrDefault();
        if (bomb != null)
        {
            var marrow = bomb.GetExtender<IMarrowEntityExtender>();
            if (marrow != null)
            {
                foreach (var (_, tag) in EntityTagManager.GetAllTags<PlayerHandTimerTag>())
                {
                    tag.Target = marrow.MarrowEntity.gameObject;
                }
                
                if (BoneStrike.Config.UseDynamicSpawns)
                    DynamicSpawnCollector.CollectAt(marrow.MarrowEntity.transform.position, BoneStrike.Config.DynamicSpawnRange);
            }
        }
        else
        {
            MelonLogger.Msg("Failed to find a bomb. This is a bug.");
        }

        Executor.RunIfHost(() =>
        {
            BoneStrike.Context.BombAudioPlayer.Start();
        });
    }

    public override void OnPlayerAction(PlayerID playerId, PlayerGameActions action, Handedness handedness)
    {
        if (!BoneStrike.Config.UseDynamicSpawns)
            return;
        
        if (!playerId.IsMe)
            return;
        
        if (action != PlayerGameActions.Dying)
            return;

        var enemyPositions = NetworkPlayer.Players
            .Where(p => p.HasRig && p.PlayerID.IsEnemy())
            .Select(p => new AvoidSpawningNear(p.RigRefs.Head.position, BoneStrike.Config.DynamicSpawnDistanceFromEnemy));

        var clockPositions = EntityTagManager
            .GetAllWithTag<BombMarker>()
            .Select(n => n.GetExtender<IMarrowEntityExtender>()?.MarrowEntity)
            .OfType<MarrowEntity>()
            .Select(m => new AvoidSpawningNear(m.transform.position, BoneStrike.Config.DynamicSpawnDistanceFromObjective));

        const int spawnSearchTries = 5;
        DynamicSpawnCollector.SetRandomSpawn(spawnSearchTries, enemyPositions.Union(clockPositions).ToArray());
    }

    private static void DropClock()
    {
        foreach (var gripWithHand in PlayerGrabManager.GetLocalHandsHoldingTag<BombMarker>())
        {
            gripWithHand.Hand.TryDetach();
        }
    }
}