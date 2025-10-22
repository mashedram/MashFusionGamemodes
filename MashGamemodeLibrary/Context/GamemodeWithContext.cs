using LabFusion.Marrow.Proxies;
using LabFusion.Menu.Data;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Menu;
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

public abstract class GamemodeWithContext<TContext, TConfig> : Gamemode
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

    protected virtual void OnRegistered()
    {
    }
    
    protected virtual void OnStart()
    {
    }

    protected virtual void OnUpdate(float delta)
    {
    }

    protected virtual void OnCleanup()
    {
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

        PlayerHider.UnhideAll();
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

    public override void OnGamemodeStarted()
    {
        base.OnGamemodeStarted();

        if (!FusionSceneManager.HasTargetLoaded()) return;

        Start();
    }

    public override void OnLevelReady()
    {
        base.OnLevelReady();

        if (!IsStarted)
            return;

        OnStart();
    }

    public override void OnGamemodeStopped()
    {
        Context.OnStop();
        
        Reset();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        
        IsStarted = base.IsStarted;
        if (!IsStarted)
            return;

        var delta = Time.deltaTime;
        
        PlayerControllerManager.Update(delta);
        GamePhaseManager.Update(delta);
        EntityTagManager.Update(delta);
        
        Context.Update(delta);
        OnUpdate(delta);
    }

    public override GroupElementData CreateSettingsGroup()
    {
        return _configMenu.GetElementData();
    }
}