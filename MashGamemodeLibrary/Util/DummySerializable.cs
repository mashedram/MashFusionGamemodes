using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Util;

public class DummySerializable : INetSerializable
{
    public int? GetSize()
    {
        return 0;
    }

    public void Serialize(INetSerializer serializer)
    {
    }
}