using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Networking.Variable.Encoder.Util;
using MashGamemodeLibrary.Registry.Typed;
using MelonLoader;

namespace MashGamemodeLibrary.Config;

public static class ConfigManager
{
    public delegate void ConfigChangedHandler(IConfig config);
    public static event ConfigChangedHandler? OnConfigChanged;
    
    // Two registries, one for our local hosted and saved config instance, and one for the config we are using and receive from the client.
    private static readonly SingletonTypedRegistry<IConfig> LocalConfigTypedRegistry = new();
    private static readonly FactoryTypedRegistry<IConfig> ActiveConfigTypedRegistry = new();
    
    private static readonly SyncedVariable<IConfig?> RemoteConfigInstance = new("ActiveConfig", new NullableReferenceEncoder<IConfig>(new DynamicInstanceEncoder<IConfig>(ActiveConfigTypedRegistry)), null);

    static ConfigManager()
    {
        RemoteConfigInstance.OnValueChanged += config =>
        {
            if (config != null) OnConfigChanged?.Invoke(config);
        };
    }
    
    public static void Register<T>() where T : IConfig, new()
    {
        LocalConfigTypedRegistry.Register<T>();
        ActiveConfigTypedRegistry.Register<T>();
    }

    internal static void Enable<T>() where T : IConfig
    {
        RemoteConfigInstance.SetAndSync(LocalConfigTypedRegistry.Get<T>());
    }
    
    public static T Get<T>() where T : class, IConfig
    {
        // If we are remote, and have a remote config, 
        if (!NetworkInfo.IsHost)
        {
            if (RemoteConfigInstance.Value is T instance)
            {
                return instance;
            }
            MelonLogger.Error("Remote config is of an invalid type.");
        }

        if (!LocalConfigTypedRegistry.TryGet<T>(out var config))
        {
            throw new Exception($"No config of type {typeof(T).Name} has been registered!");
        }

        return config;
    }

    internal static void OnValueChanged()
    {
        // We only want to sync if the gamemode is running
        // We don't have to call sync when we set the value, it automatically syncs
        if (!GamemodeManager.IsGamemodeReady)
            return;
        
        Sync();
    }

    internal static void Sync()
    {
        if (!NetworkInfo.IsHost) return;
        
        RemoteConfigInstance.Sync();
    }
}