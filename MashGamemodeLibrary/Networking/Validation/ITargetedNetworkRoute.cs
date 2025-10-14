using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;

namespace MashGamemodeLibrary.networking.Validation;

public interface ITargetedNetworkRoute : INetworkRoute
{
    public bool IsValid(byte playerIDFrom, byte playerIDTo, [MaybeNullWhen(true)] out string error);
    public MessageRoute GetTargetedMessageRoute(byte targetID);
}