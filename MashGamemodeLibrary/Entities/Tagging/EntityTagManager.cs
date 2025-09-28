using System.Diagnostics.CodeAnalysis;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.Tagging.Tags;
using MashGamemodeLibrary.networking;
using MelonLoader;

namespace MashGamemodeLibrary.Entities.Tagging;

public class SyncEntityTagPacket : INetSerializable
{
    public ushort EntityId;
    public int TagId;
    public byte[] TagData;
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EntityId);
        serializer.SerializeValue(ref TagId);
        serializer.SerializeValue(ref TagData);
    }
}

public static class EntityTagManager
{
    private static readonly Dictionary<int, Func<IEntityTag>> TagFactories = new();
    
    private static RemoteEvent<SyncEntityTagPacket> _remoteTagEvent = new(OnTagPacket, false);
    
    private static Dictionary<ushort, TrackedEntity> _entities = new();

    static EntityTagManager()
    {
        NetworkEntityManager.IDManager.OnEntityUnregistered += entity =>
        {
            _entities.Remove(entity.ID);
        };
    }
    
    public static int GetTagId<T>() where T : IEntityTag
    {
        return typeof(T).FullName?.GetHashCode() ?? throw new Exception("Failed to get hash code for tag type: " + typeof(T).FullName);
    }
    
    private static bool TryGetTrackedEntity(ushort id, [MaybeNullWhen(returnValue: false)] out TrackedEntity entity)
    {
        return _entities.TryGetValue(id, out entity);   
    }
    
    private static TrackedEntity GetOrCreateTrackedEntity(ushort id)
    {
        if (TryGetTrackedEntity(id, out var entity))
            return entity!;
        
        var temp = new TrackedEntity(id);
        _entities[id] = temp;
        return temp;
    }
        
    public static void RegisterTag<T>() where T: IEntityTag, new()
    {
        var tagId = GetTagId<T>();
#if DEBUG
        var fullname = typeof(T).FullName!;
        MelonLogger.Msg($"Registering type ${fullname} with hash {fullname.GetHashCode()} or {tagId}");
#endif
        TagFactories[tagId] = () => new T();
    }

    public static void RegisterAll<T>()
    {
        var assembly = typeof(T).Assembly;
        var method = typeof(EntityTagManager).GetMethod(nameof(RegisterTag)) ?? throw new Exception("Failed to find RegisterTag method");
        assembly.GetTypes().Where(t => typeof(IEntityTag).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false }).ForEach(t =>
        {
            method.MakeGenericMethod(t).Invoke(null, null);
        });
    }

    private static void SendTagPacket<T>(TrackedEntity entity, T tag) where T : IEntityTag, new()
    {
        var tagId = GetTagId<T>();

        SyncEntityTagPacket packet;
        if (tag is INetSerializable serializable)
        {
            using var netWriter = NetWriter.Create();
            serializable.Serialize(netWriter);
            
            packet = new SyncEntityTagPacket
            {
                EntityId = entity.GetId(),
                TagId = tagId,
                TagData = netWriter.Buffer.ToArray()
            };
        }
        else
        {
            packet = new SyncEntityTagPacket()
            {
                EntityId = entity.GetId(),
                TagId = tagId,
                TagData = Array.Empty<byte>()
            };
        }
        
        
        _remoteTagEvent.Call(packet);
    }
    
    // Remote Events

    private static void OnTagPacket(SyncEntityTagPacket packet)
    {
        if (!TagFactories.TryGetValue(packet.TagId, out var factory))
        {
            MelonLogger.Error($"Received tag packet for unregistered tag ID: {packet.TagId}. Is it registered on all clients?");
            return;
        }
        
        var tag = factory();

        if (tag is INetSerializable serializable)
        {
            using var netReader = NetReader.Create(packet.TagData);
            serializable.Serialize(netReader);
        }
        
        var trackedEntity = GetOrCreateTrackedEntity(packet.EntityId);
        trackedEntity.SetTag(tag);
    }
    
    // Network Entity Extensions

    public static void AddTag<T>(this NetworkEntity entity, T tag) where T : IEntityTag, new()
    {
        var trackedEntity = GetOrCreateTrackedEntity(entity.ID);
        
        trackedEntity.SetTag(tag);
        
        SendTagPacket(trackedEntity, tag);
    }

    public static bool HasTag<T>(this NetworkEntity entity) where T : IEntityTag
    {
        return TryGetTrackedEntity(entity.ID, out var trackedEntity) && trackedEntity.HasTag<T>();
    }

    public static bool TryGetTag<T>(this NetworkEntity entity, [MaybeNullWhen(returnValue: false)] out T tag) where T : IEntityTag
    {
        if (TryGetTrackedEntity(entity.ID, out var trackedEntity))
        {
            return trackedEntity.TryGetTag(out tag);
        }
        
        tag = default;
        return false;
    }
    
    public static List<T> GetAllExtendingTag<T>(this NetworkEntity entity) where T : IEntityTag
    {
        if (!TryGetTrackedEntity(entity.ID, out var trackedEntity))
            return new List<T>();

        return trackedEntity.GetTagsExtending<T>();
    }
    
    public static int CountEntitiesWithTag<T>() where T : IEntityTag
    {
        return _entities.Count(e => e.Value.HasTag<T>());
    }
    
    public static List<NetworkEntity> GetAllWithTag<T>(Predicate<T>? where = null) where T : IEntityTag
    {
        var list = new List<NetworkEntity>();
        
        foreach (var (_, trackedEntity) in _entities)
        {
            if (!trackedEntity.TryGetTag<T>(out var tag)) 
                continue;
            
            if (where != null && !where(tag))
                continue;
            
            if (!trackedEntity.TryGetEntity(out var entity))
                continue;
            
            list.Add(entity);
        }
        
        return list;
    }
}