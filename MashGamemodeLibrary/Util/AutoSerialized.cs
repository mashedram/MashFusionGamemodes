using System.Reflection;
using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Util;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
public class SerializableField : Attribute
{
    
}

internal class FieldSerializer
{
    private readonly FieldInfo _field;
    
    public FieldSerializer(FieldInfo field)
    {
        _field = field;
    }

    public void SerializeField(INetSerializer serializer, object instance)
    {
        var fieldValue = _field.GetValue(instance)!;
        serializer.SerializeValue(ref fieldValue);
        if (!serializer.IsReader) return;

        _field.SetValue(instance, fieldValue);
    }
}

public abstract class AutoSerialized<TValue> : INetSerializable where TValue: AutoSerialized<TValue>
{
    // This is the exact behaviour we want
    // ReSharper disable once StaticMemberInGenericType
    private static readonly List<FieldSerializer> Serializables = new();
    
    static AutoSerialized()
    {
        var type = typeof(TValue);
        var serializeAll = type.GetCustomAttribute<SerializableField>() != null;
        
        var fields = type.GetFields();
        foreach (var fieldInfo in fields)
        {
            if (!serializeAll && fieldInfo.GetCustomAttribute<SerializableField>() == null)
                continue;

            Serializables.Add(new FieldSerializer(fieldInfo));
        }
    }

    public static void Serialize(INetSerializer serializer, TValue value)
    {
        foreach (var fieldSerializer in Serializables)
        {
            fieldSerializer.SerializeField(serializer, value);
        }
    }

    public void Serialize(INetSerializer serializer)
    {
        Serialize(serializer, (TValue) this);
    }
}