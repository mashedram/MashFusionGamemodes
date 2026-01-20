using System.Reflection;
using System.Runtime.CompilerServices;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.Queries;
using MelonLoader;

namespace MashGamemodeLibrary.Util;

public static class AutoRegistery
{
    private static readonly Type[] TargetTypes = {
        typeof(ICachedQuery),
        typeof(IBehaviourCache)
    };

    private static bool HasField(Type type)
    {
        var fields = type.GetFields();
        foreach (var fieldInfo in fields)
        {
            var fieldType = fieldInfo.FieldType;
            foreach (var targetType in TargetTypes)
            {
                if (targetType.IsAssignableFrom(fieldType))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    internal static void Register(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (!HasField(type))
                continue;
            
            InternalLogger.Debug($"Registering static fields on: {type}");
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
    }

    internal static void Register<T>()
    {
        Register(typeof(T).Assembly);
    }
}