using System.Text.Json;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Networking.Variable.Encoder.Util;
using MashGamemodeLibrary.Registry.Typed;
using MelonLoader;
using MelonLoader.Utils;

namespace MashGamemodeLibrary.Config;

public static class ConfigManager
{
    public delegate void ConfigChangedHandler(IConfig config);
    public static event ConfigChangedHandler? OnConfigChanged;
    
    private static readonly string ConfigDirectoryPath = MelonEnvironment.UserDataDirectory + "/configs/";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
    };
    
    // Two registries, one for our local hosted and saved config instance, and one for the config we are using and receive from the client.
    private static readonly SingletonTypedRegistry<IConfig> LocalConfigTypedRegistry = new();
    private static readonly FactoryTypedRegistry<IConfig> ActiveConfigTypedRegistry = new();
    
    private static readonly SyncedVariable<IConfig?> RemoteConfigInstance = new("ActiveConfig", new NullableReferenceEncoder<IConfig>(new DynamicInstanceEncoder<IConfig>(ActiveConfigTypedRegistry)), null);

    private static readonly TimeSpan SyncDelay = TimeSpan.FromSeconds(3);
    private static bool IsDirty;
    private static DateTime LastChanged = DateTime.MinValue;
    
    static ConfigManager()
    {
        RemoteConfigInstance.OnValueChanged += config =>
        {
            if (config != null) OnConfigChanged?.Invoke(config);
        };
    }
    
    private static string GetConfigFilePath(Type configType)
    {
        return ConfigDirectoryPath + configType.Name + ".json";
    }
    
    private static T LoadConfig<T>() where T : class, IConfig, new()
    {
        var configType = typeof(T);
        var filePath = GetConfigFilePath(configType);

        T? config = null;
        try
        {
            Directory.CreateDirectory(filePath);

            if (File.Exists(filePath))
            {
                using var stream = File.OpenRead(filePath);
                var deserializedConfig = JsonSerializer.Deserialize<T>(stream, JsonOptions);
                if (deserializedConfig != null)
                    config = deserializedConfig;
            }
        } catch (Exception exception)
        {
            MelonLogger.Error($"Failed to load config for {configType.Name}", exception);
        }
        config ??= new T();
        
        return config;
    }
    
    private static void WriteConfig(IConfig config)
    {
        var configType = config.GetType();
        var filePath = GetConfigFilePath(configType);

        try
        {
            Directory.CreateDirectory(filePath);

            using var stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(stream, config, JsonOptions);
        }
        catch (Exception exception)
        {
            MelonLogger.Error($"Failed to write config for {configType.Name}", exception);
        }
    }
    
    public static void Register<T>() where T : class, IConfig, new()
    {
        LocalConfigTypedRegistry.Register(LoadConfig<T>());
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

    public static void Update()
    {
        if (!IsDirty) return;
        if (DateTime.Now - LastChanged < SyncDelay) return;

        Sync();
    }

    internal static void OnValueChanged()
    {
        IsDirty = true;
        LastChanged = DateTime.Now;
    }

    internal static void Sync()
    {
        IsDirty = false;
        if (!NetworkInfo.IsHost) return;
        if (RemoteConfigInstance.Value == null) return;
        
        WriteConfig(RemoteConfigInstance.Value);
        RemoteConfigInstance.Sync();
    }
}