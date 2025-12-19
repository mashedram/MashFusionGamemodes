using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable.Encoder.Impl;

public class EnumEncoder<T> : IEncoder<T> where T : struct, Enum
{
    public int GetSize(T value)
    {
        return sizeof(int);
    }

    public T Read(NetReader reader)
    {
        return reader.ReadEnum<T>();
    }
    public void Write(NetWriter writer, T value)
    {
        writer.Write(value);
    }
}