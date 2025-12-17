using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;
using LabFusion.Player;

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
    
    public bool ValidFromSender(byte id)
    {
        return PlayerIDManager.HostSmallID == id;
    }

    public MessageRoute GetMessageRoute()
    {
        return CommonMessageRoutes.ReliableToOtherClients;
    }

    public bool IsValid(byte smallIdFrom, [MaybeNullWhen(true)] out string error)
    {
        if (!NetworkValidatorHelper.IsHost(smallIdFrom))
        {
            error = $"{smallIdFrom} is not a host.";
            return false;
        }

        error = null;
        return true;
    }

    public bool IsValid(byte smallIdFrom, byte smallIDTo, [MaybeNullWhen(true)] out string error)
    {
        if (NetworkValidatorHelper.IsClient(smallIdFrom))
        {
            error = $"{smallIdFrom} is a sending as a client";
            return false;
        }

        if (NetworkValidatorHelper.IsHost(smallIDTo))
        {
            error = $"{smallIDTo} is receiving as a host";
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