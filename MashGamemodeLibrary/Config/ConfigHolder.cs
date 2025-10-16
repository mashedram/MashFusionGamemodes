using LabFusion.Network;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Variable.Impl.Var;
using MashGamemodeLibrary.Registry;

namespace MashGamemodeLibrary.Config;

public static class ConfigHolder
{
    public delegate void ConfigChangedHandler(IConfig config);
    public static event ConfigChangedHandler? OnConfigChanged;
    
    // Two registries, one for our local hosted and saved config instance, and one for the config we are using and receive from the client.
    private static readonly SingletonRegistry<IConfig> LocalConfigRegistry = new();
    private static readonly FactoryRegistry<IConfig> ActiveConfigRegistry = new();
    
    private static readonly SyncedInstance<IConfig> RemoteConfigInstance = new("ActiveConfig", ActiveConfigRegistry);

    static ConfigHolder()
    {
        RemoteConfigInstance.OnValueChanged += config =>
        {
            if (config != null) OnConfigChanged?.Invoke(config);
        };
    }
    
    public static void Register<T>() where T : IConfig, new()
    {
        LocalConfigRegistry.Register<T>();
        ActiveConfigRegistry.Register<T>();
    }

    internal static void Enable<T>() where T : IConfig
    {
        RemoteConfigInstance.Value = LocalConfigRegistry.Get<T>();
    }
    
    public static T Get<T>() where T : class, IConfig
    {
        // If we are remote, and have a remote config, 
        if (!NetworkInfo.IsHost && RemoteConfigInstance.Value is T instance)
            return instance;

        if (!LocalConfigRegistry.TryGet<T>(out var config))
        {
            throw new Exception($"No config of type {typeof(T).Name} has been registered!");
        }

        return config;
    }
}