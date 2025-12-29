using System.Reflection;
using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Context.Control;
using MashGamemodeLibrary.Execution;
using MelonLoader;

namespace MashGamemodeLibrary.Context;

public abstract class GameModeContext<TContext> where TContext : GameModeContext<TContext>
{
    private static readonly HashSet<IContextfull<TContext>> ContextCache = new();
    private static readonly HashSet<IUpdating> UpdateCache = new();

    private NetworkPlayer? _hostPlayer;
    private NetworkPlayer? _localPlayer;

    private void AddToCache<T>(ISet<T> set, FieldInfo field)
    {
        if (!typeof(T).IsAssignableFrom(field.FieldType)) 
            return;
        
#if DEBUG
        if (!field.IsInitOnly) MelonLogger.Warning($"Field: {field.Name} on context: {GetType().Name} is not read only. Updating may fail.");
#endif
        
        if (field.GetValue(this) is not T value)
            return;
        
        set.Add(value);
    }

    protected GameModeContext()
    {
        var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var field in fields)
        {
            AddToCache(UpdateCache, field);
            AddToCache(ContextCache, field);
        }
    }
    public NetworkPlayer LocalPlayer =>
        _localPlayer ?? throw new InvalidOperationException("LocalPlayer is not set. Make sure to call OnStart.");
    public NetworkPlayer HostPlayer =>
        _hostPlayer ?? throw new InvalidOperationException("Failed to get host NetworkPlayer.");

    public bool IsReady { get; private set; }
    public bool IsStarted { get; private set; }

    internal void Update(float delta)
    {
        if (!IsStarted)
            return;

        ContextCache.ForEach(entry =>
        {
            entry.Try(e => e.SetContext((TContext)this));
        });
        UpdateCache.ForEach(entry =>
        {
            entry.Try(e => e.Update(delta));
        });
    }

    internal void OnReady()
    {
        IsReady = true;
        _localPlayer = LabFusion.Player.LocalPlayer.GetNetworkPlayer();
        if (_localPlayer == null)
            throw new InvalidOperationException("Failed to get local NetworkPlayer.");

        _hostPlayer = NetworkPlayer.Players.FirstOrDefault(e => e.PlayerID.IsHost);
        if (_hostPlayer == null)
            throw new InvalidOperationException("Failed to get host NetworkPlayer.");
    }

    internal void OnStart()
    {
        IsStarted = true;
    }

    internal void OnStop()
    {
        IsStarted = false;

        foreach (var value in UpdateCache)
        {
            if (value is not IStoppable stoppable) continue;

            stoppable.Try(s => stoppable.Stop());
        }
    }

    internal void OnUnready()
    {
        IsReady = false;
    }
}