using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;

namespace MashGamemodeLibrary.networking.Validation.Routes;

public class BiDirectionalNetworkRoute : IBroadcastNetworkRoute, ITargetedNetworkRoute
{
    public string GetName()
    {
        return "Bi-directional Route";
    }

    public MessageRoute GetMessageRoute()
    {
        return CommonMessageRoutes.ReliableToOtherClients;
    }

    public bool IsValid(byte playerIDFrom, [MaybeNullWhen(returnValue: true)] out string error)
    {
        error = null;
        return true;
    }

    public bool IsValid(byte playerIDFrom, byte playerIDTo, [MaybeNullWhen(true)] out string error)
    {
        if (NetworkValidatorHelper.IsClient(playerIDFrom) == NetworkValidatorHelper.IsClient(playerIDTo))
        {
            error = $"{playerIDFrom} and {playerIDTo} are both clients.";
            return false;
        }

        error = null;
        return true;
    }

    public MessageRoute GetTargetedMessageRoute(byte targetID)
    {
        return new MessageRoute(targetID, NetworkChannel.Reliable);
    }
}