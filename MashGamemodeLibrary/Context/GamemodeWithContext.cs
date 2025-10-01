using LabFusion.SDK.Gamemodes;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Context;

public abstract class GamemodeWithContext<T> : Gamemode where T : GameContext
{
    private static T? _internalContext;
    public static T Context => _internalContext ?? throw new InvalidOperationException("Gamemode context is null. Did you forget to call base.OnGamemodeRegistered()?");
    
    public override void OnGamemodeRegistered()
    {
        _internalContext = Activator.CreateInstance<T>();
        if (_internalContext == null)
            throw new InvalidOperationException($"Failed to create instance of {typeof(T).Name}. Ensure it has a public parameterless constructor.");
        
        base.OnGamemodeRegistered();
    }

    public override void OnGamemodeReady()
    {
        Context.OnReady();
    }

    protected override void OnUpdate()
    {
        Context.Update(Time.deltaTime);
        base.OnUpdate();
    }
}