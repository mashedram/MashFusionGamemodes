using System.Diagnostics.CodeAnalysis;
using LabFusion.Player;

namespace MashGamemodeLibrary.networking.Control;

public interface IKnownSenderPacket
{
    byte SenderSmallId { get; set; }
}