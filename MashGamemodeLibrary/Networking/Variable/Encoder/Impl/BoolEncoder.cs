using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable.Encoder.Impl;

public class BoolEncoder : IEncoder<bool>
{

    public int GetSize(bool value)
    {
        return sizeof(bool);
    }

    public bool Read(NetReader reader)
    {
        return reader.ReadBoolean();
    }

    public void Write(NetWriter writer, bool value)
    {
        writer.Write(value);
    }
}