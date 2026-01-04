using BoneStrike.Audio;
using BoneStrike.Config;
using BoneStrike.Phase;
using BoneStrike.Tags;
using BoneStrike.Teams;
using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Integration;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
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
using MashGamemodeLibrary.Player.Spectating;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;
using TeamManager = MashGamemodeLibrary.Player.Team.TeamManager;

namespace BoneStrike;

public class BoneStrike : GamemodeWithContext<BoneStrikeContext, BoneStrikeConfig>
{

    private readonly PersistentTeams _teams = new();
    private Vector3 _resetPoint = Vector3.zero;
    public override string Title => "Bone Strike";
    public override string Author => "Mash";

    public override bool AutoHolsterOnDeath => true;
    public override bool DisableDevTools => Config.DevToolsDisabled;
    public override bool DisableSpawnGun => Config.DevToolsDisabled;
    public override bool DisableManualUnragdoll => true;

    public override int RoundCount => 5;

    protected override void OnRegistered()
    {
        EcsManager.RegisterAll<Mod>();
        GamePhaseManager.Registry.RegisterAll<Mod>();
        TeamManager.Registry.RegisterAll<Mod>();

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
            _teams.Clear();
            _teams.AddTeam<TerroristTeam>();
            _teams.AddTeam<CounterTerroristTeam>();
            _teams.AddPlayers(NetworkPlayer.Players.Select(p => p.PlayerID));
            _teams.RandomizeShift();
        });
    }

    protected override void OnEnd()
    {
        Executor.RunIfHost(() =>
        {
            _teams.SendMessage();
        });
    }

    protected override void OnRoundStart()
    {
        TeamManager.Enable<TerroristTeam>();
        TeamManager.Enable<CounterTerroristTeam>();

        Executor.RunIfHost(() =>
        {
            _teams.AssignAll();

            PalletLoadoutManager.Load(Config.PalletBarcodes);
            PalletLoadoutManager.LoadUtility(Config.UtilityBarcodes);
        });

        PlayerGunManager.DamageMultiplier = Config.DamageMultiplier;
        PlayerGunManager.NormalizePlayerDamage = Config.BalanceDamage;

        GamePhaseManager.Enable<PlantPhase>();

        var spawns = GamemodeMarker.FilterMarkers();

        if (spawns.Count > 0)
        {
            GamemodeHelper.SetSpawnPoints(spawns);
        }

        PlayerStatManager.SetStats(new PlayerStats
        {
            Agility = 1.2f,
            LowerStrength = 1.2f,
            UpperStrength = 1.2f,
            Speed = 1.5f,
            Vitality = 1f
        }.MulitplyHealth(Config.HealthMultiplier));

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
            _teams.AddScore(winnerTeamId, 1);
        });
    }

    protected override void OnCleanup()
    {
        LocalVision.Blind = false;
        LocalControls.LockedMovement = false;

        FusionPlayer.ResetSpawnPoints();

        Executor.RunIfHost(GameAssetSpawner.DespawnAll<BombMarker>);
    }

    public override void OnLateJoin(PlayerID playerID)
    {
        Executor.RunIfHost(() =>
        {
            playerID.SetSpectating(true);
            _teams.QueueLateJoiner(playerID);
        });

    }

    public override bool CanAttackPlayer(PlayerID player)
    {
        if (GamePhaseManager.IsPhase<PlantPhase>())
            return false;

        return !TeamManager.IsTeamMember(player);
    }

    internal static void ExplodeAllBombs()
    {
        if (!Config.BombExplosion)
            return;

        const string explosionBarcode = "BaBaCorp.MiscExplosiveDevices.Spawnable.ExplosionSmallMedDamage";

        foreach (var entry in BombMarker.Query)
        {
            if (!entry.Instance.IsReady)
                return;
            
            var position = entry.Instance.MarrowEntity.transform.position;
            GameAssetSpawner.SpawnNetworkAsset(explosionBarcode, position);
        }
    }

    private static bool AnyDefusers()
    {
        return NetworkPlayer.Players
            .Any(player =>
                player.HasRig && player.PlayerID.IsTeam<CounterTerroristTeam>() && player.HasTag<LimitedRespawnComponent>(tag => !tag.IsEliminated)
            );
    }
}