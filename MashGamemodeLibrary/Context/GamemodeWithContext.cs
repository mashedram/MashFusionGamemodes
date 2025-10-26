using LabFusion.Entities;
using LabFusion.Marrow.Proxies;
using LabFusion.Menu.Data;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.SDK.Gamemodes;
using LabFusion.Utilities;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Menu;
using MashGamemodeLibrary.Context.Helper;
using MashGamemodeLibrary.Entities.Extenders;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Vision;
using UnityEngine;
using TeamManager = MashGamemodeLibrary.Player.Team.TeamManager;

namespace MashGamemodeLibrary.Context;

public abstract class GamemodeWithContext<TContext, TConfig> : Gamemode, IOnLateJoin
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
    
    // We want it once per context
    // ReSharper disable once StaticMemberInGenericType
    public new static bool IsStarted { get; private set; }

    /// <summary>
    /// Register data related to your gamemode here.
    /// </summary>
    protected virtual void OnRegistered()
    {
    }
    
    /// <summary>
    /// Called when the gamemode starts.
    /// If a player late-joins, this also gets called once their level has fully loaded.
    /// </summary>
    protected virtual void OnStart()
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
    
    private void Start()
    {
        LocalHealth.MortalityOverride = !KnockoutAllowed;
        LocalControls.DisableSlowMo = !SlowMotionAllowed;
        
        Context.OnStart();
        OnStart();
    }

    private void Reset()
    {
        OnCleanup();
        
        GamePhaseManager.Disable();
        PlayerStatManager.ResetStats();
        GamemodeHelper.ResetSpawnPoints();
        TeamManager.Disable();

        LocalHealth.MortalityOverride = false;
        LocalControls.DisableSlowMo = false;
        
        GameObjectExtender.DestroyAll();

        PlayerHider.Reset();
        Executor.RunIfHost(() =>
        {
            PlayerControllerManager.Disable();
            EntityTagManager.ClearAll();
            SpectatorManager.Clear();
        });
    }
    
    public override void OnGamemodeRegistered()
    {
        _internalContext = Activator.CreateInstance<TContext>();
        if (_internalContext == null)
            throw new InvalidOperationException(
                $"Failed to create instance of {typeof(TContext).Name}. Ensure it has a public parameterless constructor.");

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
        Reset();

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

        IsStarted = true;
        Start();
    }

    public override void OnGamemodeStopped()
    {
        IsStarted = false;
        Context.OnStop();
        
        Reset();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        
        if (!IsStarted)
            return;

        var delta = Time.deltaTime;
        
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