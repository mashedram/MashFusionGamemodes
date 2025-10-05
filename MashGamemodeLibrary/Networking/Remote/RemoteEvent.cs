using System.Text.Json;
using System.Text.Json.Serialization;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Triggers;
using MelonLoader;

namespace MashGamemodeLibrary.networking;

public class RemoteEvent<T> : GenericRemoteEvent<T> where T : INetSerializable, new()
{
    public delegate void PacketHandler(T onEscapePointActivedPacket);
    
    private readonly PacketHandler _onEvent;
    private readonly bool _callOnSender;
    
    /**
   * An event that, when called, will run on all specified clients.
   */
    public RemoteEvent(PacketHandler onEvent, bool callOnSender) : base(
        typeof(T).FullName ?? throw new Exception("Type has no full name, cannot create RemoteEvent for it."))
    {
        _onEvent = onEvent;
        _callOnSender = callOnSender;
    }
    
    public RemoteEvent(string name, PacketHandler onEvent, bool callOnSender) : base(name)
    {
        _onEvent = onEvent;
        _callOnSender = callOnSender;
    }
    
    /**
     * Run the event on all clients connected to the server.
     * This includes the host.
     */
    public void Call(T data)
    {
        // Call it locally first
        if (_callOnSender) 
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
        if (playerId.IsMe && _callOnSender)
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
        
        Relay(data, playerId.SmallID);
    }

    protected override int? GetSize(T data)
    {
        return data.GetSize();
    }

    protected override void Write(NetWriter writer, T data)
    {
        data.Serialize(writer);
    }

    protected override void Read(NetReader reader)
    {
        var data = Activator.CreateInstance<T>();
        data.Serialize(reader);
        _onEvent.Invoke(data);
    }
}

public abstract class GenericRemoteEvent<T>
{
    private readonly ulong _assignedId;

    protected GenericRemoteEvent(string name)
    {
        _assignedId = RemoteEventMessageHandler.RegisterEvent(name, OnPacket);
    }
    
    ~GenericRemoteEvent() {
        RemoteEventMessageHandler.UnregisterEvent(_assignedId);
    }
    
    protected abstract int? GetSize(T data);
    protected abstract void Write(NetWriter writer, T data);
    protected abstract void Read(NetReader reader);
    
    protected void Relay(T data, MessageRoute route)
    {
        using var netWriter = NetWriter.Create(GetSize(data));
        Write(netWriter, data);
        RemoteEventMessageHandler.Relay(_assignedId, netWriter.Buffer, route);
    }
    
    protected void Relay(T data)
    {
        Relay(data, new MessageRoute(RelayType.ToOtherClients, NetworkChannel.Reliable));
    }

    protected void Relay(T data, byte targetId)
    {
        Relay(data, new MessageRoute(targetId, NetworkChannel.Reliable));
    }
    
    private void OnPacket(byte playerId, byte[] bytes)
    {
        using var reader = NetReader.Create(bytes);
        Read(reader);
    }
}