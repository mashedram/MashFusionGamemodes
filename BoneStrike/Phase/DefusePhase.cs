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
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.Query;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
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

        var bomb = BombMarker.Query.FirstOrDefault();
        if (bomb != null)
        {
            bomb.Instance.HookOnReady((_, marrowEntity) =>
            {
                foreach (var entry in PlayerHandTimerTag.Query)
                {
                    entry.Component.Target = marrowEntity.gameObject;
                }
                
                if (BoneStrike.Config.UseDynamicSpawns)
                    DynamicSpawnCollector.CollectAt(marrowEntity.transform.position, BoneStrike.Config.DynamicSpawnRange);
            });
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
        if (action == PlayerGameActions.Respawned && playerId.IsMe)
        {
            PalletLoadoutManager.ReassignOwnLoadout();
            return;
        }
        
        if (!BoneStrike.Config.UseDynamicSpawns)
            return;

        if (!playerId.IsMe)
            return;

        if (action != PlayerGameActions.Dying)
            return;

        var lives = EcsManager.GetComponent<LimitedRespawnComponent>(playerId.SmallID);
        if (lives is { Respawns: <= 1 })
        {
            FusionPlayer.ResetSpawnPoints();
            return;
        }

        var enemyPositions = NetworkPlayer.Players
            .Where(p => p.HasRig && p.PlayerID.IsEnemy())
            .Select(p => new AvoidSpawningNear(p.RigRefs.Head.position, BoneStrike.Config.DynamicSpawnDistanceFromEnemy));

        var clockPositions = BombMarker
            .Query
            .Where(q => q.Instance.IsReady)
            .Select(n => n.Instance.MarrowEntity)
            .Select(m => m.transform.position)
            .ToList();

        var canReach = clockPositions.FirstOrDefault();

        const int spawnSearchTries = 5;
        DynamicSpawnCollector.SetRandomSpawn(
            spawnSearchTries,
            canReach,
            enemyPositions
                .Union(
                    clockPositions
                        .Select(p => new AvoidSpawningNear(p, BoneStrike.Config.DynamicSpawnDistanceFromObjective))
                )
                .ToArray()
        );
    }

    private static void DropClock()
    {
        foreach (var gripWithHand in PlayerGrabManager.GetLocalHandsHoldingTag<BombMarker>())
        {
            gripWithHand.Hand.TryDetach();
        }
    }
}