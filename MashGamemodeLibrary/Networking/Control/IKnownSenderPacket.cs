namespace MashGamemodeLibrary.networking.Control;

public interface IKnownSenderPacket
{
    byte SenderPlayerID { get; set; }
}