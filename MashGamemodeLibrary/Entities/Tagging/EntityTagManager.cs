using System.Diagnostics.CodeAnalysis;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Registry;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Entities.Tagging;

public class SyncEntityTagPacket : INetSerializable
{
    public ushort EntityId;
    public byte[] TagData = Array.Empty<byte>();
    public ulong TagId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EntityId);
        serializer.SerializeValue(ref TagId);
        serializer.SerializeValue(ref TagData);
    }
}

// TODO: The tag system here is fucked up performance wise
// TODO: Add catchup support
public static class EntityTagManager
{
    private static readonly EntityTagSyncedDictionary Tags = new("sync.GlobalTagManager");

    // Helper Extension Map
    private static readonly Dictionary<Type, ulong> TypeToHashMap = new();
    private static readonly Dictionary<ulong, HashSet<ulong>> TagExtensionMap = new();

    // Local Cache Maps
    private static readonly Dictionary<ushort, HashSet<EntityTagIndex>> EntityToTagMap = new();
    private static readonly Dictionary<ulong, HashSet<EntityTagIndex>> TagToEntityMap = new();

    static EntityTagManager()
    {
        Tags.OnValueChanged += OnTagChanged;
        Tags.OnValueRemoved += OnTagRemoved;
    }

    // Sync Callback

    private static HashSet<EntityTagIndex> GetEntityToTagSet(ushort entityID)
    {
        if (EntityToTagMap.TryGetValue(entityID, out var set)) return set;

        var newSet = new HashSet<EntityTagIndex>();
        EntityToTagMap[entityID] = newSet;
        return newSet;
    }

    private static HashSet<EntityTagIndex> GetTagToEntitySet(ulong tagID)
    {
        if (TagToEntityMap.TryGetValue(tagID, out var set)) return set;

        var newSet = new HashSet<EntityTagIndex>();
        TagToEntityMap[tagID] = newSet;
        return newSet;
    }

    private static void OnTagChanged(EntityTagIndex key, IEntityTag value)
    {
        var entityToTag = GetEntityToTagSet(key.EntityID);
        entityToTag.Add(key);

        var tagToEntity = GetTagToEntitySet(key.TagID);
        tagToEntity.Add(key);

        if (value is ITagAdded added) added.OnAdded(key.EntityID);
    }

    private static void OnTagRemoved(EntityTagIndex key, IEntityTag oldValue)
    {
        var entityToTag = GetEntityToTagSet(key.EntityID);
        entityToTag.Remove(key);

        var tagToEntity = GetTagToEntitySet(key.TagID);
        tagToEntity.Remove(key);

        if (oldValue is ITagRemoved removed) removed.OnRemoval(key.EntityID);
    }

    // Implementations

    private static EntityTagIndex GetTagIndex<T>(NetworkEntity entity) where T : IEntityTag
    {
        return new EntityTagIndex(entity.ID, GetTagId<T>());
    }

    private static ulong GetAbstractTagId<T>() where T : IAbstractEntityTag
    {
        return typeof(T).FullName?.GetStableHash() ??
               throw new Exception("Failed to get hash code for abstract tag type: " + typeof(T).FullName);
    }

    private static ulong GetTagId<T>() where T : IEntityTag
    {
        return typeof(T).FullName?.GetStableHash() ??
               throw new Exception("Failed to get hash code for tag type: " + typeof(T).FullName);
    }

    public static void Remove(ushort id)
    {
        Executor.RunIfHost(() =>
        {
            if (!EntityToTagMap.TryGetValue(id, out var tags))
                return;

            tags.ForEach(index => Tags.Remove(index));
        });
    }

    private static HashSet<ulong> GetExtensionSet(ulong abstractTagID)
    {
        if (TagExtensionMap.TryGetValue(abstractTagID, out var set)) return set;

        var newSet = new HashSet<ulong>();
        TagExtensionMap[abstractTagID] = newSet;
        return newSet;
    }

    public static void RegisterTag<T>() where T : IEntityTag, new()
    {
        var tagID = GetTagId<T>();
        // Register its abstract extensions
        var tagType = typeof(T);
        foreach (var (key, value) in TypeToHashMap)
        {
            if (!tagType.IsAssignableTo(key)) continue;

            GetExtensionSet(value).Add(tagID);
        }


        // Register it on the network
        EntityTagSyncedDictionary.Registry.Register(tagID, () => new T());
    }

    public static void RegisterAbstractTag<T>() where T : IAbstractEntityTag
    {
        var hash = GetAbstractTagId<T>();

        TypeToHashMap[typeof(T)] = hash;
    }

    public static void RegisterAll<T>()
    {
        var assembly = typeof(T).Assembly;

        // Registering abstract tags
        var registerAbstractTypeMethod = typeof(EntityTagManager).GetMethod(nameof(RegisterAbstractTag)) ??
                                         throw new Exception("Failed to find RegisterTag method");
        assembly.GetTypes().Where(t => typeof(IAbstractEntityTag).IsAssignableFrom(t) && t.IsInterface).ForEach(t =>
        {
            registerAbstractTypeMethod.MakeGenericMethod(t).Invoke(null, null);
        });

        // Registering normal tags
        var registerTypeMethod = typeof(EntityTagManager).GetMethod(nameof(RegisterTag)) ??
                                 throw new Exception("Failed to find RegisterTag method");
        assembly.GetTypes()
            .Where(t => typeof(IEntityTag).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
            .ForEach(t => { registerTypeMethod.MakeGenericMethod(t).Invoke(null, null); });
    }

    // Network Entity Extensions

    public static void AddTag<T>(this NetworkEntity entity, T tag) where T : IEntityTag, new()
    {
        var key = GetTagIndex<T>(entity);

        Tags[key] = tag;
    }

    public static bool HasTag<T>(this NetworkEntity entity) where T : IEntityTag
    {
        var key = GetTagIndex<T>(entity);

        return Tags.ContainsKey(key);
    }

    public static bool TryGetTag<T>(this NetworkEntity entity, [MaybeNullWhen(false)] out T tag) where T : IEntityTag
    {
        var key = GetTagIndex<T>(entity);

        if (Tags.TryGetValue(key, out var foundTag))
        {
            tag = (T)foundTag;
            return true;
        }

        tag = default;
        return false;
    }

    public static List<T> GetAllExtendingTag<T>(this NetworkEntity entity) where T : IAbstractEntityTag
    {
        var extensionHash = GetAbstractTagId<T>();
        if (!TagExtensionMap.TryGetValue(extensionHash, out var extending))
        {
            MelonLogger.Error($"Tag with name: {typeof(T).FullName} is not registered");
            return new List<T>();
        }

        return extending.Select(tagID =>
        {
            var key = new EntityTagIndex(entity.ID, tagID);

            return Tags.TryGetValue(key, out var tag) ? tag : null;
        }).OfType<T>().ToList();
    }

    public static List<T> GetAllExtendingTag<T>() where T : IAbstractEntityTag
    {
        var extensionHash = GetAbstractTagId<T>();
        if (!TagExtensionMap.TryGetValue(extensionHash, out var extending))
        {
            MelonLogger.Error($"Tag with name: {typeof(T).FullName} is not registered");
            return new List<T>();
        }

        return extending.SelectMany(tagID =>
        {
            var entities = TagToEntityMap[tagID];
            return entities.Select(key => (T)Tags[key]);
        }).ToList();
    }

    public static int CountEntitiesWithTag<T>() where T : IEntityTag
    {
        var tagID = GetTagId<T>();
        return TagToEntityMap.TryGetValue(tagID, out var entities) ? entities.Count : 0;
    }

    public static HashSet<NetworkEntity> GetAllWithTag<T>(Predicate<T>? where = null) where T : IEntityTag
    {
        var tagID = GetTagId<T>();
        if (!TagToEntityMap.TryGetValue(tagID, out var entities))
            return new HashSet<NetworkEntity>();

        return entities.Select(key =>
        {
            if (!NetworkEntityManager.IDManager.RegisteredEntities.IDEntityLookup.TryGetValue(key.EntityID,
                    out var entity))
                return null;

            if (!Tags.TryGetValue(key, out var tag))
                return null;


            if (where != null)
            {
                var typedTag = (T)tag;

                if (!where.Invoke(typedTag))
                    return null;
            }

            return entity;
        }).OfType<NetworkEntity>().ToHashSet();
    }

    public static HashSet<ushort> GetAllIdsWithTag<T>(Predicate<T>? where = null) where T : IEntityTag
    {
        var tagID = GetTagId<T>();
        if (!TagToEntityMap.TryGetValue(tagID, out var entities))
            return new HashSet<ushort>();

        return entities.Where(key =>
        {
            if (!Tags.TryGetValue(key, out var tag))
                return false;


            if (where == null) return true;

            var typedTag = (T)tag;

            return where.Invoke(typedTag);
        }).Select(key => key.EntityID).ToHashSet();
    }

    public static List<KeyValuePair<NetworkEntity, T>> GetAllTags<T>(Predicate<T>? where = null) where T : IEntityTag
    {
        var tagID = GetTagId<T>();
        if (!TagToEntityMap.TryGetValue(tagID, out var entities))
            return new List<KeyValuePair<NetworkEntity, T>>();

        return entities.Select(key =>
            {
                if (!NetworkEntityManager.IDManager.RegisteredEntities.IDEntityLookup.TryGetValue(key.EntityID,
                        out var entity) || !Tags.TryGetValue(key, out var tag))
                    return (null, null);

                return (entity, tag);
            }).Where(value => value.entity != null)
            .Select(tuple => new KeyValuePair<NetworkEntity, T>(tuple.entity!, (T)tuple.tag!))
            .ToList();
    }

    public static void ClearAll()
    {
        Tags.Clear();
    }
}