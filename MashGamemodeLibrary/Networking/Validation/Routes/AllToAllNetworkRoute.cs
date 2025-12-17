using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;

namespace MashGamemodeLibrary.networking.Validation.Routes;

public class AllToAllNetworkRoute : IBroadcastNetworkRoute, ITargetedNetworkRoute
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
        return true;
    }

    public MessageRoute GetMessageRoute()
    {
        return CommonMessageRoutes.ReliableToOtherClients;
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

        error = null;
        return true;
    }

    public MessageRoute GetTargetedMessageRoute(byte targetID)
    {
        return CommonMessageRoutes.ReliableToOtherClients;
    }
}