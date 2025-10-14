using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;

namespace MashGamemodeLibrary.networking.Validation.Routes;

public class ClientToHostNetworkRoute : IBroadcastNetworkRoute, ITargetedNetworkRoute
{
    public string GetName()
    {
        return "Client To Host Route";
    }

    public MessageRoute GetMessageRoute()
    {
        return CommonMessageRoutes.ReliableToServer;
    }

    public bool IsValid(byte playerIDFrom, [MaybeNullWhen(true)] out string error)
    {
        if (!NetworkValidatorHelper.IsClient(playerIDFrom))
        {
            error = $"{playerIDFrom} is not a client";
            return false;
        }

        error = null;
        return true;
    }

    public bool IsValid(byte playerIDFrom, byte playerIDTo, [MaybeNullWhen(true)] out string error)
    {
        if (NetworkValidatorHelper.IsHost(playerIDFrom))
        {
            error = $"{playerIDFrom} is a sending as a host";
            return false;
        }

        if (NetworkValidatorHelper.IsClient(playerIDTo))
        {
            error = $"{playerIDTo} is receiving as a client";
            return false;
        }

        error = null;
        return true;
    }

    public MessageRoute GetTargetedMessageRoute(byte targetID)
    {
        return CommonMessageRoutes.ReliableToServer;
    }
}