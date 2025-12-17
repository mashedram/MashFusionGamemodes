using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;

namespace MashGamemodeLibrary.networking.Validation;

public interface ITargetedNetworkRoute : INetworkRoute
{
    public bool IsValid(byte smallIdFrom, byte smallIDTo, [MaybeNullWhen(true)] out string error);
    public MessageRoute GetTargetedMessageRoute(byte targetID);
}