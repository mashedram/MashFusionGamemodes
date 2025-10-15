using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.networking.Validation;
using MelonLoader;

namespace MashGamemodeLibrary.Networking.Remote;

public class RemoteEvent<T> : GenericRemoteEvent<T> where T : INetSerializable, new()
{
    public delegate void PacketHandler(T packet);
    private readonly PacketHandler _onEvent;

    /**
   * An event that, when called, will run on all specified clients.
   */
    public RemoteEvent(PacketHandler onEvent, INetworkRoute? route = null) : base(
        typeof(T).FullName ?? throw new Exception("Type has no full name, cannot create RemoteEvent for it."), route ?? CommonNetworkRoutes.HostToClient)
    {
        _onEvent = onEvent;
    }

    public RemoteEvent(string name, PacketHandler onEvent, INetworkRoute? route = null) : base(name,
        route ?? CommonNetworkRoutes.HostToClient)
    {
        _onEvent = onEvent;
    }

    private void OnEvent(byte sender, T data)
    {
        if (data is IKnownSenderPacket knownSenderPacket)
            knownSenderPacket.SenderPlayerID = sender;

        _onEvent.Invoke(data);
    }

    /**
     * Run the event on all clients connected to the server.
     * This includes the host.
     */
    public void Call(T data)
    {
        var localPlayer = LocalPlayer.GetNetworkPlayer();
        if (localPlayer == null)
        {
            MelonLogger.Warning("No local player found, cannot call remote event. Is there a server running?");
            return;
        }

        Relay(data);

        // Call it local as well if we need to
        if (Route.CallOnSender())
            OnEvent(PlayerIDManager.LocalSmallID, data);
    }

    public void CallFor(PlayerID playerId, T data)
    {
        // Call it locally if it's for us
        if (playerId.IsMe && Route.CallOnSender())
        {
            OnEvent(playerId.SmallID, data);
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

    protected override void Read(byte playerId, NetReader reader)
    {
        var data = Activator.CreateInstance<T>();
        data.Serialize(reader);
        OnEvent(playerId, data);
    }
}