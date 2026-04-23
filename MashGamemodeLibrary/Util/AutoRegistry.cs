using System.Reflection;
using System.Runtime.CompilerServices;

namespace MashGamemodeLibrary.Util;

[AttributeUsage(AttributeTargets.Class)]
public class RequireStaticConstructor : Attribute
{
}

public interface IGuaranteeStaticConstructor
{
}

public static class AutoRegistry
{

    private static bool HasField(Type type)
    {
        if (type.GetCustomAttribute<RequireStaticConstructor>() != null)
            return true;
        
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