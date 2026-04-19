using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Registry.Typed;
using MelonLoader;

namespace MashGamemodeLibrary.networking.Variable.Encoder.Impl;

public class InstanceEncoder<TValue> : IRefEncoder<TValue> where TValue : class, INetSerializable, new()
{
    public int GetSize(TValue value)
    {
        var selfSize = sizeof(ulong);
        if (value is INetSerializable serializable)
            selfSize += serializable.GetSize() ?? 4096;
        return selfSize;
    }

    public TValue Read(NetReader reader)
    {
        var value = new TValue();
        value.Serialize(reader);
        return value;
    }

    public void Write(NetWriter writer, TValue value)
    {
        value.Serialize(writer);
    }

    public void Serialize(INetSerializer serializer, TValue value)
    {
        value.Serialize(serializer);
    }
}