using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Variable.Encoder;

namespace MashGamemodeLibrary.Networking.Variable.Encoder.Util;

public class NetSerializableEncoder<T> : IEncoder<T> where T : INetSerializable, new()
{
    private readonly Func<T> _builder;

    public NetSerializableEncoder()
    {
        _builder = Builder;
        return;

        static T Builder()
        {
            return new T();
        }
    }

    public int GetSize(T value)
    {
        return value.GetSize() ?? 4096;
    }

    public T Read(NetReader reader)
    {
        var value = _builder.Invoke();
        value.Serialize(reader);
        return value;
    }

    public void Write(NetWriter writer, T value)
    {
        value.Serialize(writer);
    }
}