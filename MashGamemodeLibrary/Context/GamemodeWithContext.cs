using LabFusion.SDK.Gamemodes;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Context;

public abstract class GamemodeWithContext<T> : Gamemode where T : GameContext
{
    private T? _internalContext;
    protected T Context => _internalContext ?? throw new InvalidOperationException("Gamemode context is null. Did you forget to call base.OnGamemodeRegistered()?");
    
    public override void OnGamemodeRegistered()
    {
        _internalContext = Activator.CreateInstance<T>();
        base.OnGamemodeRegistered();
    }

    protected override void OnUpdate()
    {
        if (_internalContext == null)
        {
            MelonLogger.Error($"Attempted to update gamemode {Title} but context is null. Did you forget to call base.OnGamemodeRegistered()?");
            return;
        }
        
        _internalContext.Update(Time.deltaTime);
        base.OnUpdate();
    }

    /// <summary>
    /// Gets the current game context from the active gamemode.
    /// </summary>
    /// <returns>The current gamemode context</returns>
    /// <exception cref="InvalidOperationException">Thrown when the function is called outside of the gamemode</exception>
    public static T GetContext()
    {
        var gamemode = GamemodeManager.ActiveGamemode;
        var context = gamemode is GamemodeWithContext<T> gamemodeWithContext
            ? gamemodeWithContext._internalContext
            : throw new InvalidOperationException($"Active gamemode is not a {nameof(GamemodeWithContext<T>)}");
        
        if (context == null)
            throw new InvalidOperationException("Gamemode context is null. Did you forget to call base.OnGamemodeRegistered()?");

        return context;
    }
}