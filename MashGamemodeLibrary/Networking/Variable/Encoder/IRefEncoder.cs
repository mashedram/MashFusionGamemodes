using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable.Encoder;

public interface IRefEncoder<TValue> : IEncoder<TValue>
{
    void Serialize(INetSerializer serializer, TValue value);
}