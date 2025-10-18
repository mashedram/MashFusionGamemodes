using System.Reflection;
using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Util;

[AttributeUsage(AttributeTargets.Field)]
public class Synchronise : Attribute
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
        if (serializer.IsReader) return;

        _field.SetValue(instance, fieldValue);
    }
}

public abstract class AutoSerialized : INetSerializable
{
    private readonly List<FieldSerializer> _serializables = new();

    protected AutoSerialized()
    {
        var type = GetType();
        var fields = type.GetFields();
        foreach (var fieldInfo in fields)
        {
            if (fieldInfo.GetCustomAttribute<Synchronise>() == null)
                continue;

            _serializables.Add(new FieldSerializer(fieldInfo));
        }
    }
    
    public void Serialize(INetSerializer serializer)
    {
        foreach (var fieldSerializer in _serializables)
        {
            fieldSerializer.SerializeField(serializer, this);
        }
    }
}