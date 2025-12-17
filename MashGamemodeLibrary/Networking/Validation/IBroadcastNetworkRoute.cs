using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;

namespace MashGamemodeLibrary.networking.Validation;

public interface IBroadcastNetworkRoute : INetworkRoute
{
    public bool IsValid(byte smallIdFrom, [MaybeNullWhen(true)] out string error);
    public MessageRoute GetMessageRoute();
}