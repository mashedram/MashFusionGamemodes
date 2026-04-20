using BoneStrike.Audio;
using BoneStrike.Config;
using BoneStrike.Manager;
using BoneStrike.Phase;
using BoneStrike.Tags;
using BoneStrike.Teams;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.CommonComponents;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector.Weather;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;

namespace BoneStrike;

public class BoneStrike : ExtendedGamemode<BoneStrikeContext, BoneStrikeConfig>
{
    private static readonly SyncedVariable<Vector3> ResetPoint = new SyncedVariable<Vector3>("_reset.position", new Vector3Encoder(), Vector3.zero);
    public override string Title => "Bone Strike";
    public override string Author => "Mash";
    public override string LogoResource => "BoneStrike.Resources.BombIcon.png";

    public override bool AutoHolsterOnDeath => true;
    public override bool DisableDevTools => Config.DevToolsDisabled;
    public override bool DisableSpawnGun => Config.DevToolsDisabled;
    public override bool DisableManualUnragdoll => true;

    public override int RoundCount => 5;

    private bool _hasAssignedTeams;

    protected override void OnRegistered()
    {
        ResetPoint.OnValueChanged += point =>
        {
            if (!IsReady)
                return;
            
            SpawnPointHelper.SetSpawnPoint(point);
        };
        
        LimitedRespawnComponent.RegisterSpectatePredicate<BoneStrike>(_ =>
        {
            if (AnyDefusers())
                return true;

            ExplodeAllBombs();
            WinManager.Win<TerroristTeam>();
            return false;
        });

        PlayerStatisticsTracker.Register(BonestrikeStatisticsKeys.Defusals, v => v * 50);
    }

    protected override void OnStart()
    {
        Executor.RunIfHost(() =>
        {
            ResetPoint.Value = RigData.RigSpawn;
            
            PersistentTeams.Clear();
            PersistentTeams.AddTeam<TerroristTeam>();
            PersistentTeams.AddTeam<CounterTerroristTeam>();

            if (Config.ManualTeamAssignment)
            {
                _hasAssignedTeams = false;
            }
            else
            {
                PersistentTeams.AddPlayers(
                    NetworkPlayer.Players
                        .Where(p => p.HasRig)
                        .Select(p => p.PlayerID)
                );
                PersistentTeams.RandomizeShift();
                _hasAssignedTeams = true;
            }
        });
    }

    protected override void OnEnd()
    {
        Executor.RunIfHost(() =>
        {
            PersistentTeams.SendMessage();

            LeaderboardManager.ShowLeaderboard(ResetPoint);
        });
        
        FusionPlayer.ResetSpawnPoints();
    }

    protected override void OnRoundStart()
    {
        Notifier.CancelAll();
        LeaderboardManager.HideLeaderboard();
        
        SpawnPointHelper.SetSpawnPoint(ResetPoint);

        LogicTeamManager.Enable<TerroristTeam>();
        LogicTeamManager.Enable<CounterTerroristTeam>();

        Executor.RunIfHost(() =>
        {
            PalletLoadoutManager.Load(Config.PalletBarcodes);
            PalletLoadoutManager.LoadUtility(Config.UtilityBarcodes);

            if (_hasAssignedTeams)
            {
                GamePhaseManager.Enable<PlantPhase>();
            }
            else
            {
                GamePhaseManager.Enable<TeamAssignmentPhase>();
            }

            _hasAssignedTeams = true;
        });

        AvatarStatManager.BalanceStats = Config.BalanceStats;
        PlayerGunManager.NormalizePlayerDamage = Config.BalanceDamage;

        Context.EnvironmentPlayer.StartPlaying(new EnvironmentProfile<EnvironmentContext>("all",
            new EnvironmentState<EnvironmentContext>[]
            {
                new PlantState(),
                new DefuseState()
            }, LocalWeatherManager.ClearLocalWeather));
    }

    protected override void OnRoundEnd(ulong winnerTeamId)
    {
        LocalPlayer.TeleportToPosition(ResetPoint);

        Executor.RunIfHost(() =>
        {
            PersistentTeams.AddScore(winnerTeamId, 1);

            if (winnerTeamId == LogicTeamManager.GetTeamID<CounterTerroristTeam>())
            {
                Context.CounterTerroristsWinAudioPlayer.PlayRandom();
            }
            else
            {
                Context.TerroristsWinAudioPlayer.PlayRandom();
            }

            LeaderboardManager.ShowLeaderboard(ResetPoint);
        });
    }

    protected override void OnCleanup()
    {
        LocalVision.Blind = false;
        LocalControls.LockedMovement = false;
        LocalHealth.MortalityOverride = false;

        Executor.RunIfHost(GameAssetSpawner.DespawnAll<BombMarker>);
    }

    public override void OnLateJoin(PlayerID playerID)
    {
        Executor.RunIfHost(() =>
        {
            // Players joining during manual assignment should be assignable, since the game won't start without them anyway
            var activePhase = GamePhaseManager.ActivePhase;
            if (activePhase is TeamAssignmentPhase)
                return;

            playerID.SetSpectating(true);
            PersistentTeams.QueueLateJoiner(playerID);
        });
    }

    public override bool CanAttackPlayer(PlayerID player)
    {
        var activePhase = GamePhaseManager.ActivePhase;
        return activePhase switch
        {
            null => true,
            TeamAssignmentPhase => false,
            PlantPhase => false,
            DefusePhase defusePhase => defusePhase.ElapsedTime > 10f || player.IsTeam<TerroristTeam>() && !LogicTeamManager.IsTeamMember(player),
            _ => !LogicTeamManager.IsTeamMember(player)
        };
    }

    internal static void ExplodeAllBombs()
    {
        if (!Config.BombExplosion)
            return;

        const string explosionBarcode = "BaBaCorp.MiscExplosiveDevices.Spawnable.ExplosionSmallMedDamage";

        foreach (var entry in BombMarker.Query)
        {
            var transform = entry.MarrowEntity?.transform;
            if (transform == null)
                continue;

            var position = transform.position;
            GameAssetSpawner.SpawnNetworkAsset(explosionBarcode, position);
        }
    }

    internal static bool AnyDefusers()
    {
        return NetworkPlayer.Players
            .Any(player =>
                player.HasRig && player.PlayerID.IsTeam<CounterTerroristTeam>() && player.HasComponent<LimitedRespawnComponent>(tag => !tag.IsEliminated)
            );
    }
}