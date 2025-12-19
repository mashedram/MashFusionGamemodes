using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;

namespace MashGamemodeLibrary.networking.Validation.Routes;

public class AllToHostNetworkRoute : IBroadcastNetworkRoute, ITargetedNetworkRoute
{
    public string GetName()
    {
        return "Client To Host Route";
    }

    public bool CallOnSender()
    {
        return true;
    }

    public bool ValidFromSender(byte id)
    {
        return NetworkInfo.IsHost;
    }

    public MessageRoute GetMessageRoute()
    {
        return CommonMessageRoutes.ReliableToServer;
    }

    public bool IsValid(byte smallIdFrom, [MaybeNullWhen(true)] out string error)
    {
        // Any can Send

        error = null;
        return true;
    }

    public bool IsValid(byte smallIdFrom, byte smallIDTo, [MaybeNullWhen(true)] out string error)
    {
        // Any can send

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