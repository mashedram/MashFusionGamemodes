using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Variable.Encoder;

namespace MashGamemodeLibrary.Networking.Variable.Encoder.Util;

public class NullableReferenceEncoder<TValue> : IEncoder<TValue?> where TValue : class
{
    private readonly IEncoder<TValue> _childEncoder;

    public NullableReferenceEncoder(IEncoder<TValue> childEncoder)
    {
        _childEncoder = childEncoder;
    }

    public int GetSize(TValue? value)
    {
        if (value == null)
            return sizeof(bool);

        return _childEncoder.GetSize(value);
    }

    public TValue? Read(NetReader reader)
    {
        if (!reader.ReadBoolean())
            return null;

        return _childEncoder.Read(reader);
    }

    public void Write(NetWriter writer, TValue? value)
    {
        if (value == null)
        {
            writer.Write(false);
            return;
        }

        writer.Write(true);
        _childEncoder.Write(writer, value);
    }
}