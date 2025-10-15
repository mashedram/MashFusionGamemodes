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
    protected readonly INetworkRoute Route;

    protected GenericRemoteEvent(string name, INetworkRoute route)
    {
        _name = name;
        Route = route;
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
        if (Route is not IBroadcastNetworkRoute route)
        {
            MelonLogger.Error(
                $"Attempted to broadcast event: {_name} on route: {Route.GetName()}. It is not broadcasting type.");
#if DEBUG
            var stackTrace = new StackTrace();
            MelonLogger.Error(stackTrace);
#endif
            return;
        }

        var localId = PlayerIDManager.LocalSmallID;
        if (!route.IsValid(localId, out var error))
        {
            MelonLogger.Error($"Remote Event Validation error for event: {_name} on route {Route.GetName()}: {error}");
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
        if (Route is not ITargetedNetworkRoute route)
        {
            MelonLogger.Error(
                $"Attempted to broadcast event: {_name} on route: {Route.GetName()}. It is not of a targeted type.");
#if DEBUG
            var stackTrace = new StackTrace();
            MelonLogger.Error(stackTrace);
#endif
            return;
        }

        var localId = PlayerIDManager.LocalSmallID;
        if (!route.IsValid(localId, targetId, out var error))
        {
            MelonLogger.Error($"Remote Event Validation error for event: {_name} on route {Route.GetName()}: {error}");
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