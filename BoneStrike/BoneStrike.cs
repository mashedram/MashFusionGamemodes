using BoneStrike.Audio;
using BoneStrike.Config;
using BoneStrike.Phase;
using BoneStrike.Tags;
using BoneStrike.Teams;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Integration;
using LabFusion.Menu;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector.Weather;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Spectating;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;

namespace BoneStrike;

public class BoneStrike : GamemodeWithContext<BoneStrikeContext, BoneStrikeConfig>
{
    private Vector3 _resetPoint = Vector3.zero;
    public override string Title => "Bone Strike";
    public override string Author => "Mash";

    public override bool AutoHolsterOnDeath => true;
    public override bool DisableDevTools => Config.DevToolsDisabled;
    public override bool DisableSpawnGun => Config.DevToolsDisabled;
    public override bool DisableManualUnragdoll => true;

    public override int RoundCount => 5;
    
    private bool _hasAssignedTeams;

    protected override void OnRegistered()
    {
        LimitedRespawnComponent.RegisterSpectatePredicate<BoneStrike>(_ =>
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
            Context.PersistentTeams.Clear();
            Context.PersistentTeams.AddTeam<TerroristTeam>();
            Context.PersistentTeams.AddTeam<CounterTerroristTeam>();

            if (Config.ManualTeamAssignment)
            {
                _hasAssignedTeams = false;
            }
            else
            {
                Context.PersistentTeams.AddPlayers(
                    NetworkPlayer.Players
                        .Where(p => p.HasRig)
                        .Select(p => p.PlayerID)
                );
                Context.PersistentTeams.RandomizeShift();
                _hasAssignedTeams = true;
            }
        });
    }

    protected override void OnEnd()
    {
        Executor.RunIfHost(() =>
        {
            Context.PersistentTeams.SendMessage();
        });
    }

    protected override void OnRoundStart()
    {
        Notifier.CancelAll();
        LocalHealth.MortalityOverride = true;
        
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
        
        PlayerStatManager.BalanceStats = Config.BalanceStats;
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
        FusionPlayer.ResetSpawnPoints();
        LocalPlayer.TeleportToPosition(_resetPoint);

        Executor.RunIfHost(() =>
        {
            Context.PersistentTeams.AddScore(winnerTeamId, 1);

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
        LocalHealth.MortalityOverride = false;

        FusionPlayer.ResetSpawnPoints();

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
            Context.PersistentTeams.QueueLateJoiner(playerID);
        });

    }

    public override bool CanAttackPlayer(PlayerID player)
    {
        var activePhase = GamePhaseManager.ActivePhase;
        return activePhase switch
        {
            null => true,
            PlantPhase => false,
            DefusePhase defusePhase => defusePhase.ElapsedTime > 10f || player.IsTeam<TerroristTeam>(),
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
                player.HasRig && player.PlayerID.IsTeam<CounterTerroristTeam>() && player.HasTag<LimitedRespawnComponent>(tag => !tag.IsEliminated)
            );
    }
}