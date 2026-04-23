using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.Behaviour.Cache;

namespace MashGamemodeLibrary.Entities.ECS.Networking;

public interface INetworkEvents : IBehaviour
{
    void OnEvent(byte senderId, byte eventIndex, NetReader reader);
}

public static class NetworkEventsExtender
{
    private static readonly ComponentNetworkEventManager Manager = new();

    public static void SendEvent(this INetworkEvents target, byte index, int size, Action<NetWriter> writer)
    {
        Manager.Send(target, index, size, writer);
    }

    public static void Register()
    {
    }
}