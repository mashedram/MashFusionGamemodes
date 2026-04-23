using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Data.Random;
using MashGamemodeLibrary.Entities.CommonComponents;
using MashGamemodeLibrary.Environment;
using MashGamemodeLibrary.Environment.Effector.Weather;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Data.Rules.Rules;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Audio;
using TheHunt.Audio.Hunt;
using TheHunt.Config;
using TheHunt.Nightmare;
using TheHunt.Phase;
using TheHunt.Player.Speed;
using TheHunt.Teams;
using UnityEngine;

namespace TheHunt.Gamemode;

public class TheHunt : ExtendedGamemode<TheHuntContext, TheHuntConfig>
{
    public override string Title => "The Hunt";
    public override string Author => "Mash";
    
    public override bool AutoHolsterOnDeath => true;
    public override bool DisableDevTools => Config.DevToolsDisabled;
    public override bool DisableSpawnGun => Config.DevToolsDisabled;
    public override bool DisableManualUnragdoll => true;
    public override bool SlowMotionDisabled => false;


    private Vector3 _resetPoint = Vector3.zero;
    private readonly Queue<PlayerID> _nightmareQueue = new Queue<PlayerID>();
    
    public override int RoundCount => 5;
    
    
    protected override void OnRegistered()
    {
        NightmareComponent.RegisterAll<TheHunt>();
        
        LimitedRespawnComponent.RegisterSpectatePredicate<TheHunt>(player =>
        {
            if (Config.TimeGainOnKill > 0f && GamePhaseManager.IsPhase<HuntPhase>())
                HuntPhase.Extend(Config.TimeGainOnKill);
            
            if (player.HasRig)
                Context.BellAudioPlayer.PlayRandom(player.RigRefs.Head.transform.position);
            
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
        _resetPoint = RigData.RigSpawn;
        
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
        FusionPlayer.ResetSpawnPoints();
    }

    protected override void OnRoundStart()
    {
        Notifier.CancelAll();
        LocalHealth.MortalityOverride = true;
        LocalInventory.SetAmmo(2000);
        
        SpawnPointHelper.SetSpawnPoint(_resetPoint);
        
        LogicTeamManager.Enable<HiderTeam>();
        LogicTeamManager.Enable<NightmareTeam>();

        Executor.RunIfHost(() =>
        {
            PlayerDataManager.ModifyAll<PlayerCrippledRule>(playerCrippledRule => playerCrippledRule.IsEnabled = true);
            PlayerDataManager.ModifyAll<SpectatorNightvisionRule>(rule => rule.IsEnabled = Config.SpectatorNightVision);
            PlayerDataManager.ModifyAll<PlayerAmmunitionLimitRule>(rule =>
            {
                rule.AmmunitionLimit = Config.LimitMags ? Config.MagazineCapacity : null;
            });
            
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
                new HuntHiderEnvironmentState(),
                new HuntNightmareEnvironmentState(),
                new HidePhaseEnvironmentState(),
                new FinallyEnvironmentState()
            }, LocalWeatherManager.ClearLocalWeather));
    }

    protected override void OnRoundEnd(ulong winnerTeamId)
    {
        LocalPlayer.TeleportToPosition(_resetPoint);
    }

    protected override void OnCleanup()
    {
        LocalVision.Blind = false;
        LocalControls.LockedMovement = false;
        LocalControls.DisableInteraction = false;
        LocalControls.DisableInventory = false;
        LocalHealth.MortalityOverride = null;
        
        LocalSpeed.SpeedModifier = 1f;
    }

    public override void OnLateJoin(PlayerID playerID)
    {
        Executor.RunIfHost(() =>
        {
            // Players joining during manual assignment should be assignable, since the game won't start without them anyway
            var activePhase = GamePhaseManager.ActivePhase;
            if (activePhase is HidePhase)
            {
                // TODO: Allow me to define rule defaults instead of this
                playerID.Assign<HiderTeam>();
                var data = PlayerDataManager.GetPlayerData(playerID);
                if (data == null)
                    return;
                
                data.ModifyRule<PlayerCrippledRule>(playerCrippledRule => playerCrippledRule.IsEnabled = true);
                data.ModifyRule<SpectatorNightvisionRule>(rule => rule.IsEnabled = Config.SpectatorNightVision);
                data.ModifyRule<PlayerAmmunitionLimitRule>(rule =>
                {
                    rule.AmmunitionLimit = Config.LimitMags ? Config.MagazineCapacity : null;
                });
                return;
            }

            playerID.SetSpectating(true);
        });
    }

    protected override void OnPlayerLeft(PlayerID playerId)
    {
        Executor.RunIfHost(() =>
        {
            var nightmareCount = NetworkPlayer.Players.Count(p => p.HasRig && p.PlayerID.IsTeam<NightmareTeam>());
            if (nightmareCount == 0)
            {
                WinManager.Win<HiderTeam>();
                return;
            }

            var hiderCount = CountHiders();
            
            switch (hiderCount)
            {
                case 1:
                    GamePhaseManager.Enable<FinallyPhase>();
                    break;
                case 0:
                    WinManager.Win<NightmareTeam>();
                    break;
            }
        });
    }

    public override bool CanAttackPlayer(PlayerID player)
    {
        // Nightmare invincibility is handled in the team
        return !LogicTeamManager.IsTeamMember(player);
    }

    private static int CountHiders()
    {
        return NetworkPlayer.Players
            .Count(player =>
                player.HasRig && player.PlayerID.IsTeam<HiderTeam>() && player.HasComponent<LimitedRespawnComponent>(tag => !tag.IsEliminated)
            );
    }
}