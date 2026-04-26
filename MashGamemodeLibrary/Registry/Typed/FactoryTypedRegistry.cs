using System.Diagnostics.CodeAnalysis;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Registry.Typed;

public class FactoryTypedRegistry<TValue> : TypedRegistry<Func<TValue>, TValue> where TValue : class
{
    protected override Func<TValue> Create<T>()
    {
        return () => new T();
    }

    protected override bool TryToValue(Func<TValue>? from, [MaybeNullWhen(false)] out TValue value)
    {
        value = from?.Invoke();
        return value != null;
    }

    const int NullID = 0;
    private void WriteValue(NetWriter writer, TValue? value)
    {
        if (value == null)
        {
            writer.Write(NullID);
            return;
        }

        var id = GetID(value);
        writer.Write(id);
        if (value is INetSerializable serializable)
            serializable.Serialize(writer);
    }
    
    private void ReadValue(NetReader reader, ref TValue? value)
    {
        var id = reader.ReadUInt64();
        if (id == NullID)
        {
            value = null;
            return;
        }

        if (!TryGet(id, out var instance))
        {
            InternalLogger.Error($"Failed to read value with id {id} from registry {typeof(TValue).FullName}");
            value = null;
            return;
        }
        
        if (instance is INetSerializable serializable)
            serializable.Serialize(reader);

        value = instance;
    }
    
    public void SerializeValue(INetSerializer serializer, ref TValue? value)
    {
        if (serializer.IsReader)
        {
            ReadValue((NetReader)serializer, ref value);
        }
        else
        {
            WriteValue((NetWriter)serializer, value);
        }
    }
}