using System.Diagnostics;
using System.Reflection;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.networking.Validation;
using MelonLoader;

namespace MashGamemodeLibrary.Networking.Remote;

public abstract class GenericRemoteEvent<TData>
{
    private readonly ulong _assignedId;
    private readonly string _name;
    protected readonly INetworkRoute Route;

    protected GenericRemoteEvent(string name, INetworkRoute route)
    {
        _name = name;
        Route = route;

        var path = $"{GetModName(GetType())}.{name}";
        _assignedId = RemoteEventMessageHandler.RegisterEvent(path, this);
    }

    private static string GetModName(Type type)
    {
        var assembly = type.Assembly;
        var melonInfo = assembly.GetCustomAttribute<NetworkIdentifiable>();
        if (melonInfo != null)
        {
            return melonInfo.Identifier;
        }

        throw new Exception("Ensure the mod is Network Identifiable before registering it.");
    }

    ~GenericRemoteEvent()
    {
        RemoteEventMessageHandler.UnregisterEvent(_assignedId);
    }

    protected abstract int? GetSize(TData data);
    protected abstract void Write(NetWriter writer, TData data);
    protected abstract void Read(byte smallId, NetReader reader);

    private void Relay(TData data, MessageRoute route)
    {
        using var netWriter = NetWriter.Create(GetSize(data));
        Write(netWriter, data);
        RemoteEventMessageHandler.Relay(_assignedId, netWriter.Buffer, route);
    }

    protected void Relay(TData data)
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

    protected void Relay(TData data, byte targetId)
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

    internal void OnPacket(byte smallId, byte[] bytes)
    {
        if (!Route.ValidFromSender(smallId))
        {
            MelonLogger.Error(
                $"Received event from: {smallId} {(PlayerIDManager.HostSmallID == smallId ? "(Host)" : "")}. Which is invalid on route: {Route.GetType().Name}");
            return;
        }

        using var reader = NetReader.Create(bytes);
        Read(smallId, reader);
    }
}