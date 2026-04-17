using System.Reflection;
using System.Runtime.CompilerServices;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.Queries;
using MelonLoader;

namespace MashGamemodeLibrary.Util;

public interface IGuaranteeStaticConstructor
{
}

public static class AutoRegistery
{

    private static bool HasField(Type type)
    {
        var fields = type.GetFields();
        return fields.Select(fieldInfo => fieldInfo.FieldType).Any(fieldType => typeof(IGuaranteeStaticConstructor).IsAssignableFrom(fieldType));
    }

    internal static void Register(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (!HasField(type))
                continue;

            InternalLogger.Debug($"Ensuring the static on constructor is ran: {type}");
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
    }

    internal static void Register<T>()
    {
        Register(typeof(T).Assembly);
    }
}