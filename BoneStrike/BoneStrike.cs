using BoneStrike.Audio;
using BoneStrike.Config;
using BoneStrike.Manager;
using BoneStrike.Phase;
using BoneStrike.Tags;
using BoneStrike.Teams;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.CommonComponents;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector.Weather;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;

namespace BoneStrike;

public class BoneStrike : ExtendedGamemode<BoneStrikeContext, BoneStrikeConfig>
{
    public override string Title => "Bone Strike";
    public override string Author => "Mash";
    public override string LogoResource => "BoneStrike.Resources.Icon.png";

    public override bool AutoHolsterOnDeath => true;
    public override bool DisableDevTools => Config.DevToolsDisabled;
    public override bool DisableSpawnGun => Config.DevToolsDisabled;
    public override bool DisableManualUnragdoll => true;
    public override bool SlowMotionDisabled => false;

    public override int RoundCount => 5;

    private bool _hasAssignedTeams;
    private Vector3 _resetPoint = Vector3.zero;

    protected override void OnRegistered()
    {
        LimitedRespawn.RegisterSpectatePredicate<BoneStrike>(_ =>
        {
            if (AnyDefusers())
                return true;

            ExplodeAllBombs();
            WinManager.Win<TerroristTeam>();
            return false;
        });
    }

    protected override void OnStart()
    {
        _resetPoint = RigData.RigSpawn;

        Executor.RunIfHost(() =>
        {
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
        Executor.RunIfHost(PersistentTeams.SendMessage);

        LeaderboardManager.ShowLeaderboard(_resetPoint);
        Context.IntermissionMusicPlayer.Stop();
        FusionPlayer.ResetSpawnPoints();
    }

    protected override void OnRoundStart()
    {
        Notifier.CancelAll();

        SpawnPointHelper.SetSpawnPoint(_resetPoint);

        LogicTeamManager.Enable<TerroristTeam>();
        LogicTeamManager.Enable<CounterTerroristTeam>();

        Executor.RunIfHost(() =>
        {
            PlayerDataManager.ModifyAll<PlayerCrippledRule>(playerCrippledRule => playerCrippledRule.IsEnabled = Config.RemoveMovementMods);
            PlayerDataManager.ModifyAll<PlayerAmmunitionLimitRule>(rule =>
            {
                rule.AmmunitionLimit = Config.LimitMags ? Config.MagazineCapacity : null;
            });

            // Load default SLZ weapons as fallback
            var palletBarcodes = Config.PalletBarcodes;
            if (palletBarcodes.Count > 0)
            {
                PalletLoadoutManager.Load(Config.PalletBarcodes);
            }
            else
            {
                PalletLoadoutManager.Load("SLZ.BONELAB.Content");                
            }
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

        Context.IntermissionMusicPlayer.Stop();
        Context.EnvironmentPlayer.StartPlaying(new EnvironmentProfile<EnvironmentContext>("all",
            new EnvironmentState<EnvironmentContext>[]
            {
                new PlantState(),
                new DefuseState(),
                new IntermissionState()
            }, LocalWeatherManager.ClearLocalWeather));
    }

    protected override void OnRoundEnd(ulong winnerTeamId)
    {
        LocalPlayer.TeleportToPosition(_resetPoint);
        LeaderboardManager.ShowLeaderboard(_resetPoint);

        Context.IntermissionMusicPlayer.Start();

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
        });
    }

    protected override void OnCleanup()
    {
        LocalVision.Blind = false;
        LocalControls.LockedMovement = false;

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

    protected override void OnPlayerLeft(PlayerID playerId)
    {
        Executor.RunIfHost(() =>
        {
            if (GamePhaseManager.ActivePhase is not DefusePhase)
                return;

            if (AnyDefusers())
                return;

            ExplodeAllBombs();
            WinManager.Win<TerroristTeam>();
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
            DefusePhase defusePhase when player.IsTeam<CounterTerroristTeam>() => defusePhase.ElapsedTime > Config.DefuserSpawnProtection &&
                                                                                  !player.IsTeamMember(),
            _ => !player.IsTeamMember()
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
                player.HasRig && player.PlayerID.IsTeam<CounterTerroristTeam>() && player.HasComponent<LimitedRespawn>(tag => !tag.IsEliminated)
            );
    }
    
    internal static bool AnyDefenders()
    {
        return NetworkPlayer.Players
            .Any(player =>
                player.HasRig && player.PlayerID.IsTeam<TerroristTeam>() && player.HasComponent<LimitedRespawn>(tag => !tag.IsEliminated)
            );
    }
}