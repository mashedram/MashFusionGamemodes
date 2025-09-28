using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using MelonLoader;
using UnityEngine.UIElements;

namespace MashGamemodeLibrary.networking;

internal class EventMessage: INetSerializable
{
    public int EventId;
    public byte[] Payload;

    public EventMessage()
    {
        EventId = 0;
        Payload = Array.Empty<byte>();
    }
    
    public EventMessage(int eventId, byte[] payload)
    {
        EventId = eventId;
        Payload = payload;
    }

    public int? GetSize()
    {
        return sizeof(int) + sizeof(int) + Payload.Length;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EventId);
        serializer.SerializeValue(ref Payload);
    }
}

internal class RemoteEventMessageHandler : ModuleMessageHandler
{
    private static readonly Dictionary<int, Action<byte, byte[]>> EventCallbacks = new();

    public static int RegisterEvent(string name, Action<byte, byte[]> callback)
    {
        var eventId = name.GetHashCode();
        EventCallbacks[eventId] = callback;
        return eventId;
    }

    public static void UnregisterEvent(int id)
    {
        EventCallbacks.Remove(id);
    }
    
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        if (received.Route.Type == RelayType.None)
        {
            MelonLogger.Error("Received EventMessage with no relay type, cannot process.");
            return;
        }
        
        var data = received.ReadData<EventMessage>();

        if (!EventCallbacks.TryGetValue(data.EventId, out var value))
        {
            MelonLogger.Msg("Received EventMessage with unregistered event ID: " + data.EventId);
            return;
        }
        value.Invoke((byte) received.Sender!, data.Payload);
    }

    public static void Relay(int eventId, ArraySegment<byte> buffer, MessageRoute route)
    {
        if (route.Type == RelayType.None)
        {
            MelonLogger.Error("Cannot relay EventMessage with no relay type.");
            return;
        }
        
        var message = new EventMessage(eventId, buffer.ToArray());
        
        MessageRelay.RelayModule<RemoteEventMessageHandler, EventMessage>(
            message,
            route
        );
    }
}