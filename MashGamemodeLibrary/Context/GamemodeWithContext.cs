using LabFusion.Menu.Data;
using LabFusion.Player;
using LabFusion.SDK.Gamemodes;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Menu;
using MashGamemodeLibrary.Context.Helper;
using MashGamemodeLibrary.Entities.Extenders;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Phase.Rounds;
using MashGamemodeLibrary.Player.Controller;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Vision;
using UnityEngine;
using Team = MashGamemodeLibrary.Player.Team.Team;
using TeamManager = MashGamemodeLibrary.Player.Team.TeamManager;

namespace MashGamemodeLibrary.Context;

public abstract class GamemodeWithContext<TContext, TRound, TConfig> : Gamemode, IOnLateJoin, IRoundEndable
    where TContext : GameModeContext<TContext>, new()
    where TRound : RoundContext, new()
    where TConfig : class, IConfig, new()
{
    private static TContext? _internalContext;

    public static TContext Context => _internalContext ??
                                      throw new InvalidOperationException(
                                          "Gamemode context is null. Did you forget to call base.OnGamemodeRegistered()?");
    private static TRound? _internalRoundContext;
    public static TRound RoundContext => _internalRoundContext ??
                                         throw new InvalidOperationException(
                                             "Gamemode Round context is null. Did you forget to call base.OnGamemodeRegistered()?");
    
    public static TConfig Config => ConfigManager.Get<TConfig>();

    private ConfigMenu _configMenu = null!;
    
    public delegate void ConfigChangedHandler(TConfig config);
    public static event ConfigChangedHandler? OnConfigChanged;
    
    // Round systems

    private float _timeUntilNextRound = 0f;
    
    // Static on purpose
    public static bool InRound { get; private set; }
    public static int RoundIndex { get; private set; }
    
    // Extra Settings

    public virtual bool KnockoutAllowed => false;
    public virtual bool SlowMotionAllowed => false;


    private static bool _isStartedInternal;
    public new static bool IsStarted => _isStartedInternal && InRound;

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
    protected virtual void OnRoundEnd()
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

    private void StartRound()
    {
        if (RoundContext.RoundCount > 1)
        {
            Notifier.Send(new Notification
            {
                Title = "Round Start!",
                Message = $"Round: {RoundIndex + 1} / {RoundContext.RoundCount}",
                PopupLength = 4f,
                SaveToMenu = false,
                ShowPopup = true,
                Type = NotificationType.INFORMATION
            });
        }
        
        InRound = true;
        LocalHealth.MortalityOverride = !KnockoutAllowed;
        LocalControls.DisableSlowMo = !SlowMotionAllowed;
        
        Context.OnStart();
        OnRoundStart();
    }
    
    public override void OnGamemodeRegistered()
    {
        _internalContext = Activator.CreateInstance<TContext>();
        _internalRoundContext = Activator.CreateInstance<TRound>();

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

        _isStartedInternal = true;
        RoundIndex = 0;
        OnStart();
        StartRound();
    }

    public override void OnGamemodeStopped()
    {
        _isStartedInternal = false;
        
        Reset();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        
        if (!_isStartedInternal)
            return;

        var delta = Time.deltaTime;
        
        if (!InRound)
        {
            _timeUntilNextRound -= delta;
            if (_timeUntilNextRound > 0)
                return;
            
            StartRound();
            return;
        }
        
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

    public void EndRound(ulong winnerTeamId)
    {
        RoundIndex++;
        if (RoundIndex >= RoundContext.RoundCount)
        {
            GamemodeManager.StopGamemode();
            return;
        }
        
        _timeUntilNextRound = RoundContext.RoundCooldown;
        
        Notifier.Send(new Notification
        {
            Title = "Cooldown",
            Message = $"The next round will start in {MathF.Round(_timeUntilNextRound):N0} seconds",
            PopupLength = 4f,
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.INFORMATION
        });
        
        InRound = false;
        Reset();
        
        OnRoundEnd();
    }
}