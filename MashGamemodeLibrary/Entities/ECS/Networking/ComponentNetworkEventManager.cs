using LabFusion.Math;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Caches;
using MashGamemodeLibrary.Entities.ECS.Data;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.ECS.Networking;

public class NetEventCarrier
{
    public EcsIndex Index;
    public byte EventIndex;
    public int Size;
    public Action<NetWriter> Writer;
}

public class ComponentNetworkEventManager : GenericRemoteEvent<NetEventCarrier>
{
    private static readonly EcsBehaviourCache<INetworkEvents> NetworkBehaviourCache = EcsManager.CreateBehaviorCache<INetworkEvents>();
    // Handlers
    
    public ComponentNetworkEventManager() : base("sync.ECS.netevents", CommonNetworkRoutes.AllToAll)
    {
    }
    
    public void Send(INetworkEvents networkEvents, byte eventIndex, int size, Action<NetWriter> writer)
    {
        var holder = NetworkBehaviourCache.GetHolder(networkEvents);
        if (holder == null)
        {
            InternalLogger.Debug($"Could not find: {networkEvents.GetType().FullName} in lookup");
            return;
        }
        
        Relay(new NetEventCarrier()
        {
            Index = holder.Index,
            EventIndex = eventIndex,
            Size = size,
            Writer = writer
        });
    }
    
    protected override int? GetSize(NetEventCarrier data)
    {
        return data.Index.GetSize() + sizeof(byte) + data.Size;
    }
    
    protected override void Write(NetWriter writer, NetEventCarrier data)
    {
        data.Index.Serialize(writer);
        writer.Write(data.EventIndex);

        data.Writer(writer);
    }
    
    protected override void Read(byte smallId, NetReader reader)
    {
        var index = new EcsIndex();
        index.Serialize(reader);

        var instance = LocalEcsCache.GetComponentInstance(index);
        if (instance == null)
        {
            InternalLogger.Debug($"Skipping netevent on instance: {index.EntityID}, target not found.");
            return;
        }

        if (!instance.TryGetAs<INetworkEvents>(out var eventReceiver))
        {
            InternalLogger.Debug($"Received an event for: {instance.Component.GetType().FullName} which does not receive net events.");
            return;
        }
        
        eventReceiver.OnEvent(reader.ReadByte(), reader);
    }
}