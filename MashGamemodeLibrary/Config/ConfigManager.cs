using LabFusion.Network;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Variable.Impl.Var;
using MashGamemodeLibrary.Registry;
using MashGamemodeLibrary.Registry.Typed;

namespace MashGamemodeLibrary.Config;

public static class ConfigManager
{
    public delegate void ConfigChangedHandler(IConfig config);
    public static event ConfigChangedHandler? OnConfigChanged;
    
    // Two registries, one for our local hosted and saved config instance, and one for the config we are using and receive from the client.
    private static readonly SingletonTypedRegistry<IConfig> LocalConfigTypedRegistry = new();
    private static readonly FactoryTypedRegistry<IConfig> ActiveConfigTypedRegistry = new();
    
    private static readonly SyncedInstance<IConfig> RemoteConfigInstance = new("ActiveConfig", ActiveConfigTypedRegistry);

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
        RemoteConfigInstance.Value = LocalConfigTypedRegistry.Get<T>();
    }
    
    public static T Get<T>() where T : class, IConfig
    {
        // If we are remote, and have a remote config, 
        if (!NetworkInfo.IsHost && RemoteConfigInstance.Value is T instance)
            return instance;

        if (!LocalConfigTypedRegistry.TryGet<T>(out var config))
        {
            throw new Exception($"No config of type {typeof(T).Name} has been registered!");
        }

        return config;
    }

    internal static void Sync()
    {
        if (!NetworkInfo.IsHost) return;
        
        RemoteConfigInstance.Sync();
    }
}