using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.ECS.Declerations;

namespace MashGamemodeLibrary.Entities.ECS.Networking;

public interface INetworkEvents : IBehaviour
{
    void OnEvent(byte eventIndex, NetReader reader);
}

public static class NetworkEventsExtender
{
    private static readonly ComponentNetworkEventManager Manager = new ComponentNetworkEventManager();
    
    public static void SendEvent(this INetworkEvents target, byte index, int size, Action<NetWriter> writer)
    {
        Manager.Send(target, index, size, writer);
    }
    
    public static void Register() {}
}