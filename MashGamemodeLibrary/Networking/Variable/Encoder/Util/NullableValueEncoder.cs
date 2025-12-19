using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Variable.Encoder;

namespace MashGamemodeLibrary.Networking.Variable.Encoder.Util;

public class NullableValueEncoder<TValue> : IEncoder<TValue?> where TValue : struct
{
    private readonly IEncoder<TValue> _childEncoder;

    public NullableValueEncoder(IEncoder<TValue> childEncoder)
    {
        _childEncoder = childEncoder;
    }

    public int GetSize(TValue? value)
    {
        if (!value.HasValue)
            return sizeof(bool);

        return _childEncoder.GetSize(value.Value);
    }

    public TValue? Read(NetReader reader)
    {
        if (!reader.ReadBoolean())
            return null;

        return _childEncoder.Read(reader);
    }

    public void Write(NetWriter writer, TValue? value)
    {
        if (!value.HasValue)
        {
            writer.Write(false);
            return;
        }

        writer.Write(true);
        _childEncoder.Write(writer, value.Value);
    }
}