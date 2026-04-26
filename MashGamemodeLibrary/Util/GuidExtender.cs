using System.Buffers;
using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Util;

public static class GuidExtender
{
    public static void Serialize(this ref Guid guid, INetSerializer serializer)
    {
        var bytes = ArrayPool<byte>.Shared.Rent(16);
        if (serializer.IsReader)
        {
            serializer.SerializeValue(ref bytes);
            guid = new Guid(bytes);
        }
        else
        {
            guid.TryWriteBytes(bytes.AsSpan());
            serializer.SerializeValue(ref bytes);
        }
    }
}