using Il2CppSLZ.Marrow;
using LabFusion.Menu.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Menu;
using MashGamemodeLibrary.Context.Helper;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.Caches;
using MashGamemodeLibrary.Entities.Extenders;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Loadout;
using MashGamemodeLibrary.networking.Compatiblity;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Data;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Player.Team;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace MashGamemodeLibrary.Context;

public abstract class ExtendedGamemode<TContext, TConfig> : LabFusion.SDK.Gamemodes.Gamemode, IGamemode
    where TContext : GameModeContext<TContext>, new()
    where TConfig : class, IConfig, new()
{

    public delegate void ConfigChangedHandler(TConfig config);
    private static TContext? _internalContext;


    private ConfigMenu _configMenu = null!;

    public static TContext Context => _internalContext ??
                                      throw new InvalidOperationException(
                                          "Gamemode context is null. Did you forget to call base.OnGamemodeRegistered()?");

    public static TConfig Config => ConfigManager.Get<TConfig>();
    
    // Formal Settings
    
    public virtual string? LogoResource => null;
    private Texture? _logo;
    public override Texture Logo => _logo!;

    // Extra Settings

    public virtual bool KnockoutDisabled => true;
    public virtual bool SlowMotionDisabled => true;

    // Round settings

    public virtual int RoundCount => 1;
    public new static bool IsStarted { get; private set; }
    public static bool IsInRound { get; private set; }

    public static event ConfigChangedHandler? OnConfigChanged;

    // Constructor

    /// <summary>
    ///     Register data related to your gamemode here.
    /// </summary>
    protected virtual void OnRegistered()
    {
    }

    /// <summary>
    ///     Called when the gamemode first starts.
    ///     If a player late-joins, this also gets called once their level has fully loaded.
    /// </summary>
    protected virtual void OnStart()
    {

    }

    /// <summary>
    ///     Called when the gamemode ends fully.
    ///     Does not get called on round ends.
    ///     Gets called before cleanup.
    /// </summary>
    protected virtual void OnEnd()
    {

    }

    /// <summary>
    ///     Called when the round starts.
    ///     If a player late-joins, this also gets called once their level has fully loaded.
    /// </summary>
    protected virtual void OnRoundStart()
    {
    }

    /// <summary>
    ///     Gets called when the round ends naturally, with a winner.
    ///     Gets called before cleanup.
    /// </summary>
    protected virtual void OnRoundEnd(ulong winnerTeamId)
    {
    }

    /// <summary>
    ///     Called every update of the unity engine.
    /// </summary>
    /// <param name="delta">The time since the last frame</param>
    protected virtual void OnUpdate(float delta)
    {
    }

    /// <summary>
    ///     Most things are already cleaned up automatically.
    ///     Use this only to clean up either spawned instances, external side effects and other things affecting other mods.
    /// </summary>
    protected virtual void OnCleanup()
    {
    }

    /// <summary>
    ///     Called on the host when a player joins after the gamemode has started.
    /// </summary>
    /// <param name="playerID">The ID of the player that joined.</param>
    public virtual void OnLateJoin(PlayerID playerID)
    {
    }

    public virtual bool CanAttackPlayer(PlayerID player)
    {
        return true;
    }

    public void StartRound(int index)
    {
        IsInRound = true;
        Context.OnStart();
        
        // Apply settings
        LocalHealth.MortalityOverride = KnockoutDisabled ? true : null;
        LocalControls.DisableSlowMo = SlowMotionDisabled;
        
        // Start the round
        
        Executor.RunChecked(OnRoundStart);
    }

    public void EndRound(ulong winnerTeamId)
    {
        IsInRound = false;
        Executor.RunChecked(OnRoundEnd, winnerTeamId);
        Reset();
    }

    private void Reset()
    {
        Context.OnStop();
        OnCleanup();

        LocalHealth.MortalityOverride = null;
        LocalControls.DisableSlowMo = false;

        SlotData.ClearSpawned();

        GamePhaseManager.Disable();
        AvatarStatManager.ResetStats();
        PlayerGunManager.Reset();
        FusionPlayer.ResetSpawnPoints();
        LogicTeamManager.Disable();

        GameObjectExtender.DestroyAll();
        LocalEcsCache.Clear();
        NightVisionHelper.Enabled = false;
        
        // PlayerHider.Reset();
        Executor.RunIfHost(() =>
        {
            PlayerComponentExtender.ClearPlayerComponents();
            PlayerDataManager.ResetRules();
        });
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

        _logo = LogoResource != null ? ImageHelper.LoadEmbeddedImage<TContext>(LogoResource) : null;

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
        IsStarted = false;
        IsInRound = false;
    }

    public override void OnLevelReady()
    {
        base.OnLevelReady();
        
        // Zoning makes OnLevelReady get called multiple times
        if (IsStarted)
            return;

        // Reset everything on start
        Reset();

        InternalGamemodeManager.RoundCount = RoundCount;

        // Death Shenanigans
        BoneLib.Player.RigManager.GetComponent<Player_Health>().deathTimeAmount = 1f;
        // Compatibility checks
        GamemodeCompatibilityChecker.SetActiveGamemode(this);
        // Reset statistics
        PlayerStatisticsTracker.Clear();

        IsStarted = true;
        IsInRound = true;
        OnStart();
        InternalGamemodeManager.StartRound(0);
    }

    public override void OnGamemodeStopped()
    {
        IsStarted = false;
        IsInRound = false;

        GamemodeCompatibilityChecker.SetActiveGamemode(null);
        GlobalStatisticsManager.SaveStatistics(this);

        Executor.RunChecked(OnEnd);
        Reset();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (!IsStarted)
            return;

        var delta = Time.deltaTime;

        InternalGamemodeManager.Update(delta);
        GamePhaseManager.Update(delta);
        EcsManager.Update(delta);

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