using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;

namespace MashGamemodeLibrary.networking.Validation;

public interface IBroadcastNetworkRoute : INetworkRoute
{
    public bool IsValid(byte playerIDFrom, [MaybeNullWhen(true)] out string error);
    public MessageRoute GetMessageRoute();
}