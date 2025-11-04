using LabFusion.Menu.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Menu;
using MashGamemodeLibrary.Context.Helper;
using MashGamemodeLibrary.Entities.Extenders;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Player.Common;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Networking.Variable.Encoder.Util;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Phase.Rounds;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Spectating;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Util;
using MashGamemodeLibrary.Vision;
using MelonLoader;
using UnityEngine;
using Team = MashGamemodeLibrary.Player.Team.Team;
using TeamManager = MashGamemodeLibrary.Player.Team.TeamManager;

namespace MashGamemodeLibrary.Context;

struct StartRoundPacket
{
    
}

public abstract class GamemodeWithContext<TContext, TConfig> : Gamemode, IGamemode
    where TContext : GameModeContext<TContext>, new()
    where TConfig : class, IConfig, new()
{
    private static TContext? _internalContext;

    public static TContext Context => _internalContext ??
                                      throw new InvalidOperationException(
                                          "Gamemode context is null. Did you forget to call base.OnGamemodeRegistered()?");
    
    public static TConfig Config => ConfigManager.Get<TConfig>();

    private ConfigMenu _configMenu = null!;
    
    public delegate void ConfigChangedHandler(TConfig config);
    public static event ConfigChangedHandler? OnConfigChanged;
    
    // Extra Settings

    public virtual bool KnockoutAllowed => false;
    public virtual bool SlowMotionAllowed => false;
    
    // Round settings

    public virtual int RoundCount => 1;
    public virtual float TimeBetweenRounds => 30f;


    private static bool _isStartedInternal;
    public new static bool IsStarted => _isStartedInternal;
    
    // Constructor
    protected GamemodeWithContext()
    {
        
    }

    /// <summary>
    /// Register data related to your gamemode here.
    /// </summary>
    protected virtual void OnRegistered()
    {
    }

    /// <summary>
    /// Called when the gamemode first starts.
    /// If a player late-joins, this also gets called once their level has fully loaded.
    /// </summary>
    protected virtual void OnStart()
    {
        
    }

    /// <summary>
    /// Called when the gamemode ends fully.
    /// Does not get called on round ends.
    ///
    /// Gets called after cleanup.
    /// </summary>
    protected virtual void OnEnd()
    {
        
    }
    
    /// <summary>
    /// Called when the round starts.
    /// If a player late-joins, this also gets called once their level has fully loaded.
    /// </summary>
    protected virtual void OnRoundStart()
    {
    }

    /// <summary>
    /// Gets called when the round ends naturally, with a winner.
    ///
    /// Gets called after cleanup.
    /// </summary>
    protected virtual void OnRoundEnd(ulong winnerTeamId)
    {
    }

    /// <summary>
    /// Called every update of the unity engine.
    /// </summary>
    /// <param name="delta">The time since the last frame</param>
    protected virtual void OnUpdate(float delta)
    {
    }

    /// <summary>
    /// Most things are already cleaned up automatically.
    /// Use this only to clean up either spawned instances, external side effects and other things affecting other mods.
    /// </summary>
    protected virtual void OnCleanup()
    {
    }

    /// <summary>
    /// Called on the host when a player joins after the gamemode has started.
    /// </summary>
    /// <param name="playerID">The ID of the player that joined.</param>
    public virtual void OnLateJoin(PlayerID playerID)
    {
    }

    public virtual bool CanAttackPlayer(PlayerID player)
    {
        return true;
    }

    private void Reset()
    {
        Context.OnStop();
        OnCleanup();
        
        LocalHealth.MortalityOverride = null;
        LocalControls.DisableSlowMo = false;
        
        SlotData.ClearSpawned();
        
        GamePhaseManager.Disable();
        PlayerStatManager.ResetStats();
        PlayerGunManager.Reset();
        FusionPlayer.ResetSpawnPoints();
        TeamManager.Disable();
        
        GameObjectExtender.DestroyAll();
        
        PlayerHider.Reset();
        Executor.RunIfHost(() =>
        {
            PlayerTagManager.ClearPlayerTags();
            EntityTagManager.ClearAll();
            SpectatorManager.Clear();
        });
    }

    public void StartRound(int index)
    {
        // Reset statistics
        PlayerStatisticsTracker.Clear();
        PlayerDamageTracker.Reset();
        
        Context.OnStart();
        OnRoundStart();
    }
    
    public void EndRound(ulong winnerTeamId)
    {
        Reset();
        OnRoundEnd(winnerTeamId);
    }
    
    public override void OnGamemodeRegistered()
    {
        _internalContext = Activator.CreateInstance<TContext>();

        ConfigManager.Register<TConfig>();
        ConfigManager.OnConfigChanged += config =>
        {
            if (config is TConfig myConfig)
                OnConfigChanged?.Invoke(myConfig);
        };
        
        _configMenu = new ConfigMenu(Config);

        OnRegistered();
        base.OnGamemodeRegistered();
    }

    public override void OnGamemodeReady()
    {
        Executor.RunIfHost(ConfigManager.Enable<TConfig>);
        Context.OnReady();
    }

    public override void OnGamemodeUnready()
    {
        Context.OnUnready();
    }

    public override void OnLevelReady()
    {
        base.OnLevelReady();
        
        // Reset everything on start
        Reset();
        
        InternalGamemodeManager.RoundCount = RoundCount;
        InternalGamemodeManager.TimeBetweenRounds = TimeBetweenRounds;
        
        _isStartedInternal = true;
        OnStart();
        InternalGamemodeManager.StartRound(0);
    }

    public override void OnGamemodeStopped()
    {
        _isStartedInternal = false;
        
        Reset();
        OnEnd();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        
        if (!_isStartedInternal)
            return;

        var delta = Time.deltaTime;
        
        InternalGamemodeManager.Update(delta);
        GamePhaseManager.Update(delta);
        EntityTagManager.Update(delta);
        
        Context.Update(delta);
        OnUpdate(delta);
    }

    public override bool CanAttack(PlayerID player)
    {
        if (!IsStarted)
            return true;

        return CanAttackPlayer(player);
    }

    public override GroupElementData CreateSettingsGroup()
    {
        return _configMenu.GetElementData();
    }
}