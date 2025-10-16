using System.Reflection;
using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Util;

internal class FieldSerializer<T>
{
    private readonly FieldInfo _field;
    
    public FieldSerializer(FieldInfo field)
    {
        _field = field;
    }

    public void SerializeField(INetSerializer serializer, T instance)
    {
        var fieldValue = _field.GetValue(instance)!;
        serializer.SerializeValue(ref fieldValue);
        if (serializer.IsReader) return;

        _field.SetValue(instance, fieldValue);
    }
}

public class AutoSerializer<T> where T : class
{
    private readonly List<FieldSerializer<T>> _serializables = new();

    public AutoSerializer()
    {
        var type = typeof(T);
        var fields = type.GetFields();
        foreach (var fieldInfo in fields)
        {
            _serializables.Add(new FieldSerializer<T>(fieldInfo));
        }
    }
    
    // TODO: Add size
    
    public void Serialize(INetSerializer serializer, T target)
    {
        foreach (var fieldSerializer in _serializables)
        {
            fieldSerializer.SerializeField(serializer, target);
        }
    }
}