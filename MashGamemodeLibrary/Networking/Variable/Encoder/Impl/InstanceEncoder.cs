using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Registry.Typed;
using MelonLoader;

namespace MashGamemodeLibrary.networking.Variable.Encoder.Impl;

public class InstanceEncoder<TValue> : IEncoder<TValue> where TValue : class
{
    private readonly ITypedRegistry<TValue> _typedRegistry;
    
    public InstanceEncoder(ITypedRegistry<TValue> typedRegistry)
    {
        _typedRegistry = typedRegistry;
    }

    public int GetSize(TValue value)
    {
        var selfSize = sizeof(ulong);
        if (value is INetSerializable serializable)
            selfSize += serializable.GetSize() ?? 4096;
        return selfSize;
    }
    
    public TValue Read(NetReader reader)
    {
        var id = reader.ReadUInt64();
        var value = _typedRegistry.Get(id);

        switch (value)
        {
            case null:
                MelonLogger.Error($"No value registered by id {id} in {typeof(TValue).Name}'s registry.");
                return null!;
            
            case INetSerializable serializable:
                serializable.Serialize(reader);
                break;
        }

        return value;
    }
    
    public void Write(NetWriter writer, TValue value)
    {
        writer.Write(_typedRegistry.GetID(value));
        
        if (value is not INetSerializable serializable) return;
        
        serializable.Serialize(writer);
    }
}