using System.Diagnostics;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.networking.Validation;
using MelonLoader;

namespace MashGamemodeLibrary.Networking.Remote;

public abstract class GenericRemoteEvent<T>
{
    private readonly ulong _assignedId;
    private readonly string _name;
    private readonly INetworkRoute _route;

    protected GenericRemoteEvent(string name, INetworkRoute? route = null)
    {
        _name = name;
        _route = route ?? CommonNetworkRoutes.HostToClient;
        _assignedId = RemoteEventMessageHandler.RegisterEvent(name, this);
    }

    ~GenericRemoteEvent()
    {
        RemoteEventMessageHandler.UnregisterEvent(_assignedId);
    }

    protected abstract int? GetSize(T data);
    protected abstract void Write(NetWriter writer, T data);
    protected abstract void Read(byte playerId, NetReader reader);

    private void Relay(T data, MessageRoute route)
    {
        using var netWriter = NetWriter.Create(GetSize(data));
        Write(netWriter, data);
        RemoteEventMessageHandler.Relay(_assignedId, netWriter.Buffer, route);
    }

    protected void Relay(T data)
    {
        if (_route is not IBroadcastNetworkRoute route)
        {
            MelonLogger.Error(
                $"Attempted to broadcast event: {_name} on route: {_route.GetName()}. It is not broadcasting type.");
#if DEBUG
            var stackTrace = new StackTrace();
            MelonLogger.Error(stackTrace);
#endif
            return;
        }

        var localId = PlayerIDManager.LocalSmallID;
        if (!route.IsValid(localId, out var error))
        {
            MelonLogger.Error($"Remote Event Validation error for event: {_name} on route {_route.GetName()}: {error}");
#if DEBUG
            var stackTrace = new StackTrace();
            MelonLogger.Error(stackTrace);
#endif
            return;
        }

        Relay(data, route.GetMessageRoute());
    }

    protected void Relay(T data, byte targetId)
    {
        if (_route is not ITargetedNetworkRoute route)
        {
            MelonLogger.Error(
                $"Attempted to broadcast event: {_name} on route: {_route.GetName()}. It is not of a targeted type.");
#if DEBUG
            var stackTrace = new StackTrace();
            MelonLogger.Error(stackTrace);
#endif
            return;
        }

        var localId = PlayerIDManager.LocalSmallID;
        if (!route.IsValid(localId, targetId, out var error))
        {
            MelonLogger.Error($"Remote Event Validation error for event: {_name} on route {_route.GetName()}: {error}");
#if DEBUG
            var stackTrace = new StackTrace();
            MelonLogger.Error(stackTrace);
#endif
            return;
        }

        Relay(data, route.GetTargetedMessageRoute(targetId));
    }

    internal void OnPacket(byte playerId, byte[] bytes)
    {
        using var reader = NetReader.Create(bytes);
        Read(playerId, reader);
    }
}