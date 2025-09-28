using System.Text.Json;
using System.Text.Json.Serialization;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Triggers;
using MelonLoader;

namespace MashGamemodeLibrary.networking;

public class RemoteEvent<T> where T : INetSerializable, new()
{
    private readonly int _assignedId;
    private readonly string _name;
    private readonly Action<T> _onEvent;
    private readonly bool _callOnHost = true;

    public RemoteEvent(string name, Action<T> onEvent, bool callOnHost = true)
    {
        _name = name;
        _onEvent = onEvent;
        _callOnHost = callOnHost;
        
        _assignedId = RemoteEventMessageHandler.RegisterEvent(_name, OnPacket);
    }
    
    ~RemoteEvent() {
        RemoteEventMessageHandler.UnregisterEvent(_assignedId);
    }
    
    /**
     * An event that, when called, will run on all specified clients.
     */
    public RemoteEvent(Action<T> onEvent, bool callOnHost = true) : this(
        typeof(T).FullName ?? throw new Exception("Type has no full name, cannot create RemoteEvent for it."), 
        onEvent,
        callOnHost)
    {
    }
    
    private void Relay(T packet, MessageRoute route)
    {
        using var netWriter = NetWriter.Create(packet.GetSize());
        packet.Serialize(netWriter);
        RemoteEventMessageHandler.Relay(_assignedId, netWriter.Buffer, route);
    }
    
    private void Relay(T packet)
    {
        Relay(packet, new MessageRoute(RelayType.ToOtherClients, NetworkChannel.Reliable));
    }

    private void Relay(byte targetId, T packet)
    {
        Relay(packet, new MessageRoute(targetId, NetworkChannel.Reliable));
    }

    /**
     * Run the event on all clients connected to the server.
     * This includes the host.
     */
    public void Call(T data)
    {
        // Call it locally first
        if (_callOnHost) 
            _onEvent(data);
        
        var localPlayer = LocalPlayer.GetNetworkPlayer();
        if (localPlayer == null)
        {
            MelonLogger.Warning("No local player found, cannot call remote event. Is there a server running?");
            return;
        }
        
        Relay(data);
    }
    
    public void CallFor(PlayerID playerId, T data)
    {
        // Call it locally if it's for us
        if (playerId.IsMe && _callOnHost)
        {
            _onEvent.Invoke(data);
            return;
        }
        
        var localPlayer = LocalPlayer.GetNetworkPlayer();
        if (localPlayer == null)
        {
            MelonLogger.Warning("No local player found, cannot call remote event. Is there a server running?");
            return;
        }
        
        Relay(playerId.SmallID, data);
    }

    private void OnPacket(byte b, byte[] bytes)
    {
        using var serializer = NetReader.Create(bytes);
        var serializable = default(T)!;
        serializable.Serialize(serializer);
        _onEvent.Invoke(serializable);
    }
}