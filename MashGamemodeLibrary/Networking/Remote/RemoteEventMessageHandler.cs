using System.Security.Cryptography;
using System.Text;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using MashGamemodeLibrary.Util;
using MelonLoader;
using UnityEngine.UIElements;

namespace MashGamemodeLibrary.networking;

#if DEBUG
class InvalidRemoteEventPacket : INetSerializable
{
    private const int IdSize = sizeof(ulong);
    
    public ulong EventId;
    public byte[] KnownIdBytes;
    
    public ulong[] GetKnownIds()
    {
        var count = KnownIdBytes.Length / IdSize;
        var ids = new ulong[count];
        for (var i = 0; i < count; i++)
        {
            ids[i] = BitConverter.ToUInt64(KnownIdBytes, i * IdSize);
        }

        return ids;
    }
    
    public InvalidRemoteEventPacket()
    {
        EventId = 0;
        KnownIdBytes = Array.Empty<byte>();
    }
    
    public InvalidRemoteEventPacket(ulong eventId, ulong[] knownIds)
    {
        EventId = eventId;
        KnownIdBytes = new byte[knownIds.Length * IdSize];
        for (var i = 0; i < knownIds.Length; i++)
        {
            var bytes = BitConverter.GetBytes(knownIds[i]);
            Array.Copy(bytes, 0, KnownIdBytes, i * IdSize, IdSize);
        }
    }
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EventId);
        serializer.SerializeValue(ref KnownIdBytes);
    }
}
#endif

internal class EventMessage: INetSerializable
{
    public ulong EventId;
    public byte[] Payload;

    public EventMessage()
    {
        EventId = 0;
        Payload = Array.Empty<byte>();
    }
    
    public EventMessage(ulong eventId, byte[] payload)
    {
        EventId = eventId;
        Payload = payload;
    }

    public int? GetSize()
    {
        return sizeof(ulong) + sizeof(int) + Payload.Length;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EventId);
        serializer.SerializeValue(ref Payload);
    }
}

internal class RemoteEventMessageHandler : ModuleMessageHandler
{
    private static readonly Dictionary<ulong, Action<byte, byte[]>> EventCallbacks = new();
    
#if DEBUG
    private static readonly Dictionary<ulong, string> EventNames = new();
    private static readonly RemoteEvent<InvalidRemoteEventPacket> _onInvalidEventPacket = new("RML_InvalidRemoteEventPacket", OnInvalidEventPacket, false);
    
    private static void OnInvalidEventPacket(InvalidRemoteEventPacket packet)
    {
        var eventId = packet.EventId;
        if (!EventNames.TryGetValue(eventId, out var name))
        {
            MelonLogger.Msg($"Received invalid RemoteEvent with unknown ID: {eventId}. Did we even send it?");
            return;
        }
        
        var knownIds = packet.GetKnownIds();
        var knownNames = knownIds.Select(id => EventNames.TryGetValue(id, out var knownName) ? $"{id} - {knownName}" : $"Unknown ({id})").ToList();
        MelonLogger.Msg($"Received invalid RemoteEvent with ID: {eventId} ({name}). Known IDs: {string.Join(", ", knownNames)}");
    }
#endif

    public static ulong RegisterEvent(string name, Action<byte, byte[]> callback)
    {
        var eventId = StableHash.Fnv1A64(name);
        EventCallbacks[eventId] = callback;
        #if DEBUG
        EventNames[eventId] = name; 
        MelonLogger.Msg($"Registered RemoteEvent with name: {name} and ID: {eventId}");
        #endif
        return eventId;
    }

    public static void UnregisterEvent(ulong id)
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
        #if DEBUG
        try
        {
#endif
            if (!EventCallbacks.TryGetValue(data.EventId, out var value))
            {
                MelonLogger.Msg("Received EventMessage with unregistered event ID: " + data.EventId);
#if DEBUG
                var host = NetworkPlayer.Players.FirstOrDefault(e => e.PlayerID.IsHost);
                if (host == null)
                {
                    throw new Exception(
                        "How the fuck do we receive a network event without a host to send it. WHAT THE FUCK");
                }

                _onInvalidEventPacket.CallFor(host.PlayerID,
                    new InvalidRemoteEventPacket(data.EventId, EventCallbacks.Keys.ToArray()));
#endif
                return;
            }

            value.Invoke((byte)received.Sender!, data.Payload);
#if DEBUG
        }
        catch (Exception e)
        {
            var name = EventNames[data.EventId];
            MelonLogger.Msg($"Event with id: {data.EventId} - {name} had threw an exception: {e}");
        }
#endif
    }

    public static void Relay(ulong eventId, ArraySegment<byte> buffer, MessageRoute route)
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