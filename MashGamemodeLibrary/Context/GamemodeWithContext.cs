using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Entities.Tagging;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Context;

public abstract class GamemodeWithContext<T> : Gamemode where T : GameContext, new()
{
    private static T? _internalContext;
    public static T Context => _internalContext ?? throw new InvalidOperationException("Gamemode context is null. Did you forget to call base.OnGamemodeRegistered()?");
    public static bool IsStarted { get; private set; }
    
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

    protected override void OnUpdate()
    {
        IsStarted = base.IsStarted;
        if (!IsStarted)
            return;
        
        Context.Update(Time.deltaTime);
        base.OnUpdate();
    }
}