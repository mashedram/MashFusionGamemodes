using System.Reflection;
using System.Runtime.CompilerServices;
using BoneLib;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Modules;
using LabFusion.Utilities;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Validation.Routes;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.networking;

#if DEBUG
internal class InvalidRemoteEventPacket : INetSerializable
{
    private const int IdSize = sizeof(ulong);

    public ulong EventId;
    public byte[] KnownIdBytes;

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

    public ulong[] GetKnownIds()
    {
        var count = KnownIdBytes.Length / IdSize;
        var ids = new ulong[count];
        for (var i = 0; i < count; i++) ids[i] = BitConverter.ToUInt64(KnownIdBytes, i * IdSize);

        return ids;
    }
}
#endif

internal class RemoteSceneLoadedPacket : DummySerializable, IKnownSenderPacket
{
    public byte SenderPlayerID { get; set; }
}

internal class EventMessage : INetSerializable
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

public class RemoteEventMessageHandler : ModuleMessageHandler
{
    private static readonly Dictionary<ulong, Action<byte, byte[]>> EventCallbacks = new();

    private static readonly List<ICatchup> Catchups = new();
    private static readonly List<IResettable> Resettables = new();

    // Due to the order in which static fields initialize, this NEEDS to be the lowest one.
    private static readonly Dictionary<ulong, string> EventNames = new();

    private static readonly RemoteEvent<InvalidRemoteEventPacket> OnInvalidEventPacket =
        new("RML_InvalidRemoteEventPacket", OnInvalidRemoteEvent, CommonNetworkRoutes.ClientToHost);

    private static readonly RemoteEvent<RemoteSceneLoadedPacket> LevelLoadedEvent =
        new("RML_LevelLoadedEvent", OnRemoteLevelLoader, new ClientToHostNetworkRoute());

    static RemoteEventMessageHandler()
    {
        MultiplayerHooking.OnJoinedServer += OnServerChanged;
        MultiplayerHooking.OnStartedServer += OnServerChanged;
        MultiplayerHooking.OnDisconnected += OnServerChanged;

        Hooking.OnLevelLoaded += _ =>
        {
            if (!NetworkInfo.HasServer)
                return;

            if (NetworkInfo.IsHost)
                return;

            LevelLoadedEvent.CallFor(PlayerIDManager.GetHostID(), new RemoteSceneLoadedPacket());
        };
    }

    public static ulong RegisterEvent<T>(string name, GenericRemoteEvent<T> callback)
    {
        var eventId = StableHash.Fnv1A64(name);
        EventCallbacks[eventId] = callback.OnPacket;
#if DEBUG
        EventNames[eventId] = name;
        MelonLogger.Msg($"Registered RemoteEvent with name: {name} and ID: {eventId}");
#endif

        if (callback is ICatchup catchup) Catchups.Add(catchup);

        if (callback is IResettable resettable) Resettables.Add(resettable);

        return eventId;
    }

    /// <summary>
    ///     This registers all *STATIC* RemoteEvents in the assembly of type T.
    /// </summary>
    /// <typeparam name="T">The mod class type</typeparam>
    public static void RegisterMod<T>()
    {
        foreach (var type in typeof(T).Assembly.GetTypes())
        {
            if (type.IsGenericType)
                continue;

            var foundRemoteEvent = false;
            foreach (var field in type.GetFields(
                         BindingFlags.Public |
                         BindingFlags.NonPublic |
                         BindingFlags.Static
                     ))
            {
                var fieldType = field.FieldType;

                var baseType = fieldType.BaseType;
                while (baseType != null)
                {
                    if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(GenericRemoteEvent<>))
                    {
                        foundRemoteEvent = true;
                        break;
                    }

                    baseType = baseType.BaseType;
                }

                if (foundRemoteEvent)
                    break;
            }

            if (!foundRemoteEvent)
                continue;

            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
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
                var hostID = PlayerIDManager.GetHostID();
                if (hostID == null)
                    throw new Exception(
                        "How the fuck do we receive a network event without a host to send it. WHAT THE FUCK");

                OnInvalidEventPacket.CallFor(hostID,
                    new InvalidRemoteEventPacket(data.EventId, EventCallbacks.Keys.ToArray()));
#endif
                return;
            }

            var senderId = (byte)received.Sender!;
            value.Invoke(senderId, data.Payload);
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

    private static void OnRemoteLevelLoader(RemoteSceneLoadedPacket packet)
    {
        Executor.RunIfHost(() =>
        {
            var id = PlayerIDManager.GetPlayerID(packet.SenderPlayerID);

            if (id == null)
                return;

            foreach (var catchup in Catchups) catchup.OnCatchup(id);
        });
    }

    private static void OnServerChanged()
    {
        Resettables.ForEach(r => r.Reset());
    }

#if DEBUG


    private static void OnInvalidRemoteEvent(InvalidRemoteEventPacket packet)
    {
        var eventId = packet.EventId;
        if (!EventNames.TryGetValue(eventId, out var name))
        {
            MelonLogger.Msg($"Received invalid RemoteEvent with unknown ID: {eventId}. Did we even send it?");
            return;
        }

        var knownIds = packet.GetKnownIds();
        var knownNames = knownIds.Select(id =>
            EventNames.TryGetValue(id, out var knownName) ? $"{id} - {knownName}" : $"Unknown ({id})").ToList();
        MelonLogger.Msg(
            $"Received invalid RemoteEvent with ID: {eventId} ({name}). Known IDs: {string.Join(", ", knownNames)}");
    }


#endif
}