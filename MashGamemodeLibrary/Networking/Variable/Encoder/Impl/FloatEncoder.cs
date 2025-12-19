using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable.Encoder.Impl;

public class FloatEncoder : IEncoder<float>
{

    public int GetSize(float value)
    {
        return sizeof(float);
    }

    public float Read(NetReader reader)
    {
        return reader.ReadSingle();
    }

    public void Write(NetWriter writer, float value)
    {
        writer.Write(value);
    }
}