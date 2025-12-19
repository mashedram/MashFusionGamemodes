using MashGamemodeLibrary.Registry.Keyed;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Registry.Generic;

public class RuntimeTypedRegistry : KeyedRegistry<ulong, object>, IKeyable<object>
{
    private readonly Type RootType;

    public RuntimeTypedRegistry(Type rootType)
    {
        RootType = rootType;
    }

    public ulong CreateID<T>() where T : notnull
    {
        return CreateID(typeof(T));
    }

    public ulong CreateID(object instance)
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

        var id = CreateID<T>();
        Register(id, value);
    }

    public object? Get<T>() where T : notnull
    {
        var id = CreateID<T>();
        return base.Get(id);
    }
}