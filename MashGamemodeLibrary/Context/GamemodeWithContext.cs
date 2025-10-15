using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.SDK.Gamemodes;
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

public abstract class GamemodeWithContext<T> : Gamemode where T : GameModeContext, new()
{
    private static T? _internalContext;

    public static T Context => _internalContext ??
                               throw new InvalidOperationException(
                                   "Gamemode context is null. Did you forget to call base.OnGamemodeRegistered()?");
    // Extra Settings

    public virtual bool KnockoutAllowed => false;
    public virtual bool SlowMotionAllowed => false;  
    
    // We want it once per context
    // ReSharper disable once StaticMemberInGenericType
    public new static bool IsStarted { get; private set; }

    protected virtual void OnStart()
    {
    }

    protected virtual void OnUpdate(float delta)
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
        _internalContext = Activator.CreateInstance<T>();
        if (_internalContext == null)
            throw new InvalidOperationException(
                $"Failed to create instance of {typeof(T).Name}. Ensure it has a public parameterless constructor.");

        base.OnGamemodeRegistered();
    }

    public override void OnGamemodeReady()
    {
        Reset();

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
        
        Context.Update(delta);
        OnUpdate(delta);
    }
}