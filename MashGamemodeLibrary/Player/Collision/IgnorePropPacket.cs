using LabFusion.Entities;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Control;

namespace MashGamemodeLibrary.Player.Collision;

internal class IgnorePropPacket : INetSerializable, IKnownSenderPacket
{
    public byte SenderPlayerID { get; set; }
    public NetworkEntityReference Reference;
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Reference);
    }
}