using LabFusion.Network;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Registry;
using MelonLoader;

namespace MashGamemodeLibrary.Config;

internal class ConfigPacket  : INetSerializable
{
    private ulong _configID;
    public IConfigInstance? ConfigInstance;

    public ConfigPacket()
    {
        ConfigInstance = null;
    }
    
    public ConfigPacket(IConfigInstance? configInstance)
    {
        if (configInstance != null)
            _configID = ConfigHolder.ActiveConfigRegistry.GetID(configInstance);
        ConfigInstance = configInstance;
    }

    public int? GetSize()
    {
        return sizeof(ulong) + ConfigInstance?.GetSize() ?? 0;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _configID);
        if (serializer.IsReader)
        {
            ConfigInstance = ConfigHolder.ActiveConfigRegistry.Get(_configID) ?? throw new Exception($"Failed to find config of id: {_configID}");
        }

        ConfigInstance?.Serialize(serializer);
    }
}

public static class ConfigHolder
{
    // Two registries, one for our local hosted and saved config instance, and one for the config we are using and receive from the client.
    private static readonly SingletonRegistry<IConfigInstance> LocalConfigRegistry = new();
    internal static readonly FactoryRegistry<IConfigInstance> ActiveConfigRegistry = new();
    private static IConfigInstance? _remoteConfigInstance;
    
    private static readonly RemoteEvent<ConfigPacket> ConfigUpdateEvent = new("mgl_config_update", OnConfigReceived, CommonNetworkRoutes.HostToAll);

    public static void Register<T>() where T : IConfigInstance, new()
    {
        LocalConfigRegistry.Register<T>();
        ActiveConfigRegistry.Register<T>();
    }

    internal static void Enable<T>() where T : IConfigInstance
    {
        _remoteConfigInstance = ActiveConfigRegistry.Get<T>();
    }
    
    public static T Get<T>() where T : class, IConfigInstance
    {
        // If we are remote, and have a remote config, 
        if (!NetworkInfo.IsHost && _remoteConfigInstance is T instance)
            return instance;

        if (!LocalConfigRegistry.TryGet<T>(out var config))
        {
            throw new Exception($"No config of type {typeof(T).Name} has been registered!");
        }

        return config;
    }
    
    // Events
    
    private static void OnConfigReceived(ConfigPacket packet)
    {
        _remoteConfigInstance = packet.ConfigInstance;
    }
}