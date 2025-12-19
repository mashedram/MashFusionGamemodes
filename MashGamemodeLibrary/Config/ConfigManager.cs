using System.Text.Json;
using LabFusion.Network;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Networking.Variable.Encoder.Util;
using MashGamemodeLibrary.Registry.Typed;
using MelonLoader;

namespace MashGamemodeLibrary.Config;

public static class ConfigManager
{
    public delegate void ConfigChangedHandler(IConfig config);

    private static readonly string ConfigDirectoryPath = Mod.ModDataDirectory + "/configs/";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };

    // Two registries, one for our local hosted and saved config instance, and one for the config we are using and receive from the client.
    private static readonly SingletonTypedRegistry<IConfig> LocalConfigTypedRegistry = new();
    private static readonly FactoryTypedRegistry<IConfig> ActiveConfigTypedRegistry = new();

    private static readonly SyncedVariable<IConfig?> RemoteConfigInstance = new("ActiveConfig",
        new NullableReferenceEncoder<IConfig>(new DynamicInstanceEncoder<IConfig>(ActiveConfigTypedRegistry)), null);

    private static readonly TimeSpan SyncDelay = TimeSpan.FromSeconds(5);
    private static readonly HashSet<IConfig> _dirtyConfigs = new();
    private static DateTime LastChanged = DateTime.MinValue;

    static ConfigManager()
    {
        RemoteConfigInstance.OnValueChanged += config =>
        {
            if (config != null) OnConfigChanged?.Invoke(config);
        };
    }
    public static event ConfigChangedHandler? OnConfigChanged;

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
            if (File.Exists(filePath))
            {
                using var stream = File.OpenRead(filePath);
                var deserializedConfig = JsonSerializer.Deserialize<T>(stream, JsonOptions);
                if (deserializedConfig != null)
                    config = deserializedConfig;
            }
        }
        catch (Exception exception)
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
            Directory.CreateDirectory(ConfigDirectoryPath);

            using var stream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            stream.SetLength(0);
            JsonSerializer.Serialize(stream, config, configType, JsonOptions);
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
        if (_dirtyConfigs.Count == 0) return;
        if (DateTime.Now - LastChanged < SyncDelay) return;

        Save();
        Sync();
    }

    internal static void OnValueChanged(IConfig config)
    {
        _dirtyConfigs.Add(config);
        LastChanged = DateTime.Now;
    }

    internal static void Save()
    {
        foreach (var dirtyConfig in _dirtyConfigs)
        {
            WriteConfig(dirtyConfig);
        }
        _dirtyConfigs.Clear();
    }

    internal static void Sync()
    {
        if (!NetworkInfo.IsHost) return;
        if (RemoteConfigInstance.Value == null) return;

        RemoteConfigInstance.Sync();
    }
}