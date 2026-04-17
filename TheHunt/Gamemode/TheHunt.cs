using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Data.Random;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector.Weather;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Audio;
using TheHunt.Audio.Hunt;
using TheHunt.Config;
using TheHunt.Nightmare;
using TheHunt.Phase;
using TheHunt.Teams;
using UnityEngine;

namespace TheHunt.Gamemode;

public class TheHunt : GamemodeWithContext<TheHuntContext, TheHuntConfig>
{
    public override string Title => "The Hunt";
    public override string Author => "Mash";
    
    public override bool AutoHolsterOnDeath => true;
    public override bool DisableDevTools => Config.DevToolsDisabled;
    public override bool DisableSpawnGun => Config.DevToolsDisabled;
    public override bool DisableManualUnragdoll => true;

    private static readonly SyncedVariable<Vector3> ResetPoint = new SyncedVariable<Vector3>("_reset.position", new Vector3Encoder(), Vector3.zero);
    private readonly Queue<PlayerID> _nightmareQueue = new Queue<PlayerID>();
    
    public override int RoundCount => 5;
    
    
    protected override void OnRegistered()
    {
        NightmareComponent.RegisterAll<TheHunt>();
        
        LimitedRespawnComponent.RegisterSpectatePredicate<TheHunt>(_ =>
        {
            var hiders = CountHiders();
            if (hiders > 0)
            {
                if (hiders == 1)
                    GamePhaseManager.Enable<FinallyPhase>();
                return true;
            }

            WinManager.Win<NightmareTeam>();
            return false;
        });
    }

    protected override void OnStart()
    {
        ResetPoint.Value = RigData.RigSpawn;

        Executor.RunIfHost(() =>
        {
            _nightmareQueue.Clear();
            // Add all players to the queue, shuffled so a player doesn't get biased
            foreach (var playerID in PlayerIDManager.PlayerIDs.Shuffle())
            {
                _nightmareQueue.Enqueue(playerID);
            }
        });
    }

    protected override void OnEnd()
    {
        
    }

    protected override void OnRoundStart()
    {
        Notifier.CancelAll();
        LocalHealth.MortalityOverride = true;
        
        LogicTeamManager.Enable<HiderTeam>();
        LogicTeamManager.Enable<NightmareTeam>();

        Executor.RunIfHost(() =>
        {
            GamePhaseManager.Enable<HidePhase>();
            
            // Assign nightmare
            if (!_nightmareQueue.TryDequeue(out var nightmareID))
            {
                // No nightmare, refresh the queue
                foreach (var p in PlayerIDManager.PlayerIDs.Shuffle())
                {
                    _nightmareQueue.Enqueue(p);
                }

                // Always has a value, due to PlayerIDs never being empty
                nightmareID = _nightmareQueue.Dequeue();
            }
        
            foreach (var playerID in PlayerIDManager.PlayerIDs)
            {
                if (!playerID.IsValid)
                    return;
                
                if (playerID.Equals(nightmareID))
                {
                    playerID.Assign<NightmareTeam>();
                }
                else
                {
                    playerID.Assign<HiderTeam>();    
                }
            }
        });
        
        AvatarStatManager.BalanceStats = Config.BalanceStats;
        
        EnvironmentContext.Reset();
        Context.EnvironmentPlayer.StartPlaying(new EnvironmentProfile<EnvironmentContext>("night",
            new EnvironmentState<EnvironmentContext>[]
            {
                new ChaseEnvironmentState(),
                new HuntEnvironmentState(),
                new HidePhaseEnvironmentState(),
                new FinallyEnvironmentState()
            }, LocalWeatherManager.ClearLocalWeather));
    }

    protected override void OnRoundEnd(ulong winnerTeamId)
    {
        FusionPlayer.ResetSpawnPoints();
        LocalPlayer.TeleportToPosition(ResetPoint);
    }

    protected override void OnCleanup()
    {
        LocalVision.Blind = false;
        LocalControls.LockedMovement = false;
        LocalHealth.MortalityOverride = false;
    }

    public override void OnLateJoin(PlayerID playerID)
    {
        Executor.RunIfHost(() =>
        {
            // Players joining during manual assignment should be assignable, since the game won't start without them anyway
            var activePhase = GamePhaseManager.ActivePhase;
            if (activePhase is HidePhase)
                return;

            playerID.SetSpectating(true);
        });
    }
    
    public override bool CanAttackPlayer(PlayerID player)
    {
        if (LogicTeamManager.IsLocalTeam<NightmareTeam>())
            return true;

        return false;
    }


    private static int CountHiders()
    {
        return NetworkPlayer.Players
            .Count(player =>
                player.HasRig && player.PlayerID.IsTeam<HiderTeam>() && player.HasComponent<LimitedRespawnComponent>(tag => !tag.IsEliminated)
            );
    }
}