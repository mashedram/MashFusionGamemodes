using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable.Encoder.Impl;

public class ByteEncoder : IEncoder<byte>
{
    public int GetSize(byte value)
    {
        return sizeof(byte);
    }

    public byte Read(NetReader reader)
    {
        return reader.ReadByte();
    }

    public void Write(NetWriter writer, byte value)
    {
        writer.Write(value);
    }
}