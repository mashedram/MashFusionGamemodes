using MashGamemodeLibrary.Registry;

namespace MashGamemodeLibrary.Config;

public static class ConfigHolder
{
    public static readonly FactoryRegistry<IConfigInstance> Registry = new();
    private static IConfigInstance? _configInstance;

    internal static void Enable<T>() where T : IConfigInstance
    {
        _configInstance = Registry.Get<T>();
    }
    
    public static T Get<T>() where T : IConfigInstance
    {
        if (_configInstance is T instance)
            return instance;
        
        throw new Exception($"Config instance is not of type {typeof(T).Name}! Make sure to activate it.");
    }
}