using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable.Encoder;

public interface IEncoder<TValue>
{
    int GetSize(TValue value);
    TValue Read(NetReader reader);
    void Write(NetWriter riter, TValue value);
}