using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable.Encoder.Impl;

public class ULongEncoder : IEncoder<ulong>
{

    public int GetSize(ulong value)
    {
        return sizeof(ulong);
    }
    public ulong Read(NetReader reader)
    {
        return reader.ReadUInt64();
    }
    public void Write(NetWriter writer, ulong value)
    {
        writer.Write(value);
    }
}