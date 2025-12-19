using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable.Encoder.Impl;

public class IntEncoder : IEncoder<int>
{
    public int GetSize(int value)
    {
        return sizeof(int);
    }

    public int Read(NetReader reader)
    {
        return reader.ReadInt32();
    }

    public void Write(NetWriter writer, int value)
    {
        writer.Write(value);
    }
}