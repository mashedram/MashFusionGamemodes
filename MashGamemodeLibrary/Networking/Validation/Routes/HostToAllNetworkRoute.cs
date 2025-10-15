using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;

namespace MashGamemodeLibrary.networking.Validation.Routes;

public class HostToAllNetworkRoute : IBroadcastNetworkRoute, ITargetedNetworkRoute
{
    public string GetName()
    {
        return "Host To Client Route";
    }

    public bool CallOnSender()
    {
        return true;
    }

    public MessageRoute GetMessageRoute()
    {
        return CommonMessageRoutes.ReliableToOtherClients;
    }

    public bool IsValid(byte playerIDFrom, [MaybeNullWhen(true)] out string error)
    {
        if (!NetworkValidatorHelper.IsHost(playerIDFrom))
        {
            error = $"{playerIDFrom} is not a host.";
            return false;
        }

        error = null;
        return true;
    }

    public bool IsValid(byte playerIDFrom, byte playerIDTo, [MaybeNullWhen(true)] out string error)
    {
        if (NetworkValidatorHelper.IsClient(playerIDFrom))
        {
            error = $"{playerIDFrom} is a sending as a client";
            return false;
        }

        if (NetworkValidatorHelper.IsHost(playerIDTo))
        {
            error = $"{playerIDTo} is receiving as a host";
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