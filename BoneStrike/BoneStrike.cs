using System.Collections.Immutable;
using BoneStrike.Audio;
using BoneStrike.Config;
using BoneStrike.Phase;
using BoneStrike.Tags;
using BoneStrike.Teams;
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
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Stats;
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
    public override bool DisableManualUnragdoll => Config.DevToolsDisabled;

    private readonly HashSet<PlayerID> _terroristsSet = new();
    private readonly HashSet<PlayerID> _counterTerroristsSet = new();

    protected override void OnRegistered()
    {
        EntityTagManager.RegisterAll<Mod>();
        GamePhaseManager.Registry.RegisterAll<Mod>();
        TeamManager.Registry.RegisterAll<Mod>();
    }

    protected override void OnStart()
    {
        _terroristsSet.Clear();
        _counterTerroristsSet.Clear();
        
        Executor.RunIfHost(() =>
        {
            var players = NetworkPlayer.Players.Select(p => p.PlayerID).Shuffle().ToList();
            var team1Size = players.Count / 2;

            var team1 = players.Take(team1Size);
            var team2 = players.Skip(team1Size);
        
            foreach (var playerID in team1)
            {
                _terroristsSet.Add(playerID);
            }

            foreach (var playerID in team2)
            {
                _counterTerroristsSet.Add(playerID);
            }
        });
    }

    private void Assign(PlayerID playerID, bool terrorist)
    {
        if (terrorist)
            TeamManager.Assign<TerroristTeam>(playerID);
        else 
            TeamManager.Assign<CounterTerroristTeam>(playerID);
    }
    
    private void Assign(HashSet<PlayerID> players, bool terrorist)
    {
        foreach (var playerID in players)
        {
            Assign(playerID, terrorist);
        }
    }
    
    protected override void OnRoundStart()
    {
        TeamManager.Enable<TerroristTeam>();
        TeamManager.Enable<CounterTerroristTeam>();

        Executor.RunIfHost(() =>
        {
            _terroristsSet.RemoveWhere(p => !p.IsValid);
            _counterTerroristsSet.RemoveWhere(p => !p.IsValid);
            var swapTeams = RoundIndex % 2 == 1;
            Assign(_terroristsSet, swapTeams);
            Assign(_counterTerroristsSet, !swapTeams);
        });
        
        PlayerControllerManager.Enable(() => new LimitedRespawnTag(Config.MaxRespawns));
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

    protected override void OnRoundEnd()
    {
        GamemodeHelper.TeleportToSpawnPoint();
    }

    protected override void OnCleanup()
    {
        LocalVision.Blind = false;
        LocalControls.LockedMovement = false;
        
        GamemodeHelper.ResetSpawnPoints();

        GameAssetSpawner.DespawnAll<BombMarker>();
    }

    public override void OnLateJoin(PlayerID playerID)
    {
        Executor.RunIfHost(() =>
        {
            var swapTeams = RoundIndex % 2 == 1;
            if (_terroristsSet.Count >= _counterTerroristsSet.Count)
            {
                _terroristsSet.Add(playerID);
                Assign(playerID, swapTeams);
            }
            else
            {
                _counterTerroristsSet.Add(playerID);
                Assign(playerID, !swapTeams);
            }
        });
    }

    public override bool CanAttackPlayer(PlayerID player)
    {
        if (GamePhaseManager.IsPhase<PlantPhase>())
            return false;

        return !TeamManager.IsTeamMember(player);
    }
}