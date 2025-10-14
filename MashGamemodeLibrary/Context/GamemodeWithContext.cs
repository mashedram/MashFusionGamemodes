using LabFusion.Scene;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Context;

public abstract class GamemodeWithContext<T> : Gamemode where T : GameModeContext, new()
{
    private static T? _internalContext;
    public static T Context => _internalContext ?? throw new InvalidOperationException("Gamemode context is null. Did you forget to call base.OnGamemodeRegistered()?");
    
    // We want it once per context
    // ReSharper disable once StaticMemberInGenericType
    public new static bool IsStarted { get; private set; }

    protected virtual void OnStart() {}
    
    public override void OnGamemodeRegistered()
    {
        _internalContext = Activator.CreateInstance<T>();
        if (_internalContext == null)
            throw new InvalidOperationException($"Failed to create instance of {typeof(T).Name}. Ensure it has a public parameterless constructor.");
        
        base.OnGamemodeRegistered();
    }

    public override void OnGamemodeReady()
    {
        EntityTagManager.ClearAll();
        
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
        Context.OnStart();
        OnStart();
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
    }

    protected override void OnUpdate()
    {
        IsStarted = base.IsStarted;
        if (!IsStarted)
            return;
        
        Context.Update(Time.deltaTime);
        base.OnUpdate();
    }
}