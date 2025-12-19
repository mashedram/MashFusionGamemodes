using System.Diagnostics.CodeAnalysis;
using LabFusion.Network;
using LabFusion.Player;

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

    public bool ValidFromSender(byte id)
    {
        return PlayerIDManager.HostSmallID == id && !NetworkInfo.IsHost || PlayerIDManager.HostSmallID != id && NetworkInfo.IsHost;
    }

    public bool IsValid(byte smallIdFrom, [MaybeNullWhen(true)] out string error)
    {
        error = null;
        return true;
    }

    public bool IsValid(byte smallIdFrom, byte smallIDTo, [MaybeNullWhen(true)] out string error)
    {
        if (NetworkValidatorHelper.IsClient(smallIdFrom) == NetworkValidatorHelper.IsClient(smallIDTo))
        {
            error = $"{smallIdFrom} and {smallIDTo} are both clients.";
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