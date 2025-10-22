using MashGamemodeLibrary.Registry.Keyed;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Registry.Generic;

public class RuntimeTypedRegistry : KeyedRegistry<ulong, object>, IKeyable<object>
{
    private Type RootType;
    
    public RuntimeTypedRegistry(Type rootType)
    {
        RootType = rootType;
    }

    public ulong GetID<T>() where T : notnull
    {
        return GetID(typeof(T));
    }
    
    public ulong GetID(object instance)
    {
        return instance.GetType().Name.GetStableHash();
    }
    
    public void Register<T>(object value) where T : notnull
    {
        var wantedType = RootType.MakeGenericType(typeof(T));

        if (!value.GetType().IsAssignableFrom(wantedType))
        {
            MelonLogger.Error($"Failed to register: {value.GetType().Name}. Expected a value of: {wantedType.Name}");
            return;
        }
        
        var id = GetID<T>();
        Register(id, value);
    }

    public object? Get<T>()
    {
        var id = GetID<T>();
        return base.Get(id);
    }
}