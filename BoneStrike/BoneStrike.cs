using System.Collections.Immutable;
using BoneStrike.Audio;
using BoneStrike.Config;
using BoneStrike.Phase;
using BoneStrike.Tags;
using BoneStrike.Teams;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.MLAgents;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Integration;
using LabFusion.Player;
using LabFusion.RPC;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Data.Random;
using MashGamemodeLibrary.Entities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector.Weather;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Spectating;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;
using Random = UnityEngine.Random;
using TeamManager = MashGamemodeLibrary.Player.Team.TeamManager;

namespace BoneStrike;

public class BoneStrike : GamemodeWithContext<BoneStrikeContext, BonestrikeRound, BoneStrikeConfig>
{
    public override string Title => "Bone Strike";
    public override string Author => "Mash";
    
    public override bool AutoHolsterOnDeath => true;
    public override bool DisableDevTools => Config.DevToolsDisabled;
    public override bool DisableSpawnGun => Config.DevToolsDisabled;
    public override bool DisableManualUnragdoll => true;

    private readonly PersistentTeams _teams = new();

    protected override void OnRegistered()
    {
        EntityTagManager.RegisterAll<Mod>();
        GamePhaseManager.Registry.RegisterAll<Mod>();
        TeamManager.Registry.RegisterAll<Mod>();
    }

    protected override void OnStart()
    {
        Executor.RunIfHost(() =>
        {
            _teams.Clear();
            _teams.AddTeam<TerroristTeam>();
            _teams.AddTeam<CounterTerroristTeam>();
            _teams.AddPlayers(NetworkPlayer.Players.Select(p => p.PlayerID));
        });
    }

    protected override void OnEnd()
    {
        Executor.RunIfHost(() =>
        {
            _teams.SendMessage();
        });

        PlayerStatisticsTracker.SendNotificationAndAwardBits(PlayerDamageStatistics.Kills, PlayerDamageStatistics.Assists);
    }

    protected override void OnRoundStart()
    {
        TeamManager.Enable<TerroristTeam>();
        TeamManager.Enable<CounterTerroristTeam>();
        
        Executor.RunIfHost(() =>
        {
            _teams.AssignAll();
            
            PalletLoadoutManager.Load(Config.PalletBarcode);
            
            LimitedRespawnTag.SetSpectatePredicate(player =>
            {
                if (AnyDefusers(player))
                    return true;

                ExplodeAllBombs();
                WinManager.Win<TerroristTeam>();
                return false;
            });
        });
        
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
            Vitality = 1.5f
        });
        
        Context.EnvironmentPlayer.StartPlaying(new EnvironmentProfile<EnvironmentContext>("all",
            new EnvironmentState<EnvironmentContext>[]
            {
                new PlantState(),
                new DefuseState()
            }, LocalWeatherManager.ClearLocalWeather));
    }

    protected override void OnRoundEnd(ulong winnerTeamId)
    {
        GamemodeHelper.ResetSpawnPoints();
        GamemodeHelper.TeleportToSpawnPoint();
        Executor.RunIfHost(() =>
        {
            _teams.AddScore(winnerTeamId, 1);
        });
    }

    protected override void OnCleanup()
    {
        LocalVision.Blind = false;
        LocalControls.LockedMovement = false;
        
        GamemodeHelper.ResetSpawnPoints();

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
        const string explosionBarcode = "BaBaCorp.MiscExplosiveDevices.Spawnable.ExplosionMedBigDamge";
        
        foreach (var networkEntity in EntityTagManager.GetAllWithTag<BombMarker>())
        {
            var marrow = networkEntity.GetExtender<IMarrowEntityExtender>();
            if (marrow == null) continue;

            var position = marrow.MarrowEntity.transform.position;
            GameAssetSpawner.SpawnNetworkAsset(explosionBarcode, position);
        }
    }
    
    private static bool AnyDefusers(NetworkPlayer? skip = null)
    {
        return NetworkPlayer.Players.Any(player => 
            player.HasRig && player.PlayerID.IsTeam<CounterTerroristTeam>() && !player.PlayerID.IsSpectating() && !player.PlayerID.Equals(skip?.PlayerID)
        );
    }
}