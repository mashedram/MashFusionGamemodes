using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;
using LabFusion.Player;

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

    public bool ValidFromSender(byte id)
    {
        return PlayerIDManager.HostSmallID != id;
    }

    public bool IsValid(byte smallIdFrom, [MaybeNullWhen(true)] out string error)
    {
        if (!NetworkValidatorHelper.IsClient(smallIdFrom))
        {
            error = $"{smallIdFrom} is not a client";
            return false;
        }

        error = null;
        return true;
    }

    public bool IsValid(byte smallIdFrom, byte smallIDTo, [MaybeNullWhen(true)] out string error)
    {
        if (NetworkValidatorHelper.IsHost(smallIdFrom))
        {
            error = $"{smallIdFrom} is a sending as a host";
            return false;
        }

        if (NetworkValidatorHelper.IsClient(smallIDTo))
        {
            error = $"{smallIDTo} is receiving as a client";
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