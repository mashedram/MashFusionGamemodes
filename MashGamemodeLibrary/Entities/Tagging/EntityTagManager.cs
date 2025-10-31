using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Networking.Variable.Encoder.Util;
using MashGamemodeLibrary.Registry;
using MashGamemodeLibrary.Registry.Typed;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Entities.Tagging;

public record struct EntityTagIndex(ushort EntityID, ulong TagID) : INetSerializable
{
    public ushort EntityID = EntityID;
    public ulong TagID = TagID;

    public int? GetSize()
    {
        return sizeof(ushort) + sizeof(ulong);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EntityID);
        serializer.SerializeValue(ref TagID);
    }
}

public interface ITagCache
{
    void Add(EntityTagIndex index, IEntityTag tag);

    void Remove(EntityTagIndex index, IEntityTag tag);
    void Clear();
}

public class TagCache<TTag> : ITagCache, IEnumerable<TTag> where TTag : class
{
    private HashSet<TTag> _cache = new();
    
    public void Add(EntityTagIndex index, IEntityTag tag)
    {
        if (tag is not TTag tag2)
            return;

        _cache.Add(tag2);
    }

    public void Remove(EntityTagIndex index, IEntityTag tag)
    {
        if (tag is not TTag tag2)
            return;

        _cache.Remove(tag2);
    }
    
    public void Clear()
    {
        _cache.Clear();
    }

    public IEnumerator<TTag> GetEnumerator()
    {
        return _cache.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class EntityTagCache<TTag> : ITagCache where TTag : class
{
    private readonly Dictionary<ushort, HashSet<TTag>> _cache = new();
    
    private HashSet<TTag> GetSet(ushort entityID)
    {
        if (_cache.TryGetValue(entityID, out var set)) return set;

        var newSet = new HashSet<TTag>();
        _cache[entityID] = newSet;
        return newSet;
    }

    
    public void Add(EntityTagIndex index, IEntityTag tag)
    {
        if (tag is not TTag tag2)
            return;

        var entityID = index.EntityID;
        GetSet(entityID).Add(tag2);
    }

    public void Remove(EntityTagIndex index, IEntityTag tag)
    {
        var entityID = index.EntityID;
        if (!_cache.TryGetValue(entityID, out var set))
            return;
        
        if (tag is not TTag tag2)
            return;

        set.Remove(tag2);
        if (set.Count == 0)
            _cache.Remove(entityID);
    }
    
    public void Clear()
    {
        _cache.Clear();
    }
    
    public HashSet<TTag>? Get(ushort index)
    {
        return _cache.GetValueOrDefault(index);
    }

    public bool TryGet(ushort entityID, [MaybeNullWhen(false)] out HashSet<TTag> tags)
    {
        return _cache.TryGetValue(entityID, out tags);
    }
}

class UpdateTagCache : TagCache<ITagUpdate>
{
    public void Call(float delta)
    {
        foreach (var entry in this)
        {
            entry.Update(delta);
        }
    }
}

public static class EntityTagManager
{
    private static readonly FactoryTypedRegistry<IEntityTag> Registry = new();
    private static readonly SyncedDictionary<EntityTagIndex, IEntityTag> Tags = new("sync.GlobalTagManager", new NetSerializableEncoder<EntityTagIndex>(), new DynamicInstanceEncoder<IEntityTag>(Registry));
    
    // Helper Extension Map
    private static readonly Dictionary<Type, ulong> TypeToHashMap = new();
    private static readonly Dictionary<ulong, HashSet<ulong>> TagExtensionMap = new();

    // Local Cache Maps
    private static readonly Dictionary<ushort, HashSet<EntityTagIndex>> EntityToTagMap = new();
    private static readonly Dictionary<ulong, HashSet<EntityTagIndex>> TagToEntityMap = new();
    
    // Special Tags
    private static readonly SingletonTypedRegistry<ITagCache> TypedTagCache = new();
    // Updates every frame, don't want to hash and stuff just to access it
    private static readonly UpdateTagCache DirectUpdateTagCache;

    static EntityTagManager()
    {
        Tags.OnValueAdded += OnTagAdded;
        Tags.OnValueChanged += OnTagChanged;
        Tags.OnValueRemoved += OnTagRemoved;
        Tags.OnValueCleared += OnTagsCleared;
        
        TypedTagCache.Register<UpdateTagCache>();
        DirectUpdateTagCache = TypedTagCache.Get<UpdateTagCache>() ?? throw new InvalidOperationException("Failed to get update tag cache");
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

    private static void OnTagAdded(EntityTagIndex key, IEntityTag value)
    {
        var entityToTag = GetEntityToTagSet(key.EntityID);
       
        entityToTag.Add(key);

        var tagToEntity = GetTagToEntitySet(key.TagID);
        tagToEntity.Add(key);

        if (value is ITagAddedInternal entity) 
            entity.OnAddInternal(key);
        
        foreach (var (_, cache) in TypedTagCache)
        {
            cache.Add(key, value);
        }
        
        if (value is ITagAdded added)
            added.OnAdded(key.EntityID);
    }

    private static void OnTagChanged(EntityTagIndex index, IEntityTag value)
    {
        if (value is ITagChanged changed)
            changed.OnChanged();
    }

    private static void OnTagRemoved(EntityTagIndex key, IEntityTag oldValue)
    {
        var entityToTag = GetEntityToTagSet(key.EntityID);
        entityToTag.Remove(key);

        var tagToEntity = GetTagToEntitySet(key.TagID);
        tagToEntity.Remove(key);

        foreach (var (_, cache) in TypedTagCache)
        {
            cache.Remove(key, oldValue);
        }
        
        if (oldValue is ITagRemoved removed)
            removed.OnRemoval(key.EntityID);
    }

    private static void OnTagsCleared()
    {
        EntityToTagMap.Clear();
        TagToEntityMap.Clear();
        foreach (var (_, cache) in TypedTagCache)
        {
            cache.Clear();
        }

    }

    // Implementations

    private static EntityTagIndex GetTagIndex<T>(this NetworkEntity entity) where T : IEntityTag
    {
        return new EntityTagIndex(entity.ID, GetTagId<T>());
    }
    
    private static EntityTagIndex GetTagIndex(this NetworkEntity entity, IEntityTag tag)
    {
        return new EntityTagIndex(entity.ID, GetTagId(tag.GetType()));
    }

    private static ulong GetAbstractTagId<T>() where T : IAbstractEntityTag
    {
        return typeof(T).FullName?.GetStableHash() ??
               throw new Exception("Failed to get hash code for abstract tag type: " + typeof(T).FullName);
    }

    private static ulong GetTagId<T>() where T : IEntityTag
    {
        return Registry.CreateID<T>();
    }
    
    private static ulong GetTagId(Type type)
    {
        return Registry.CreateID(type);
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
        Registry.Register<T>();
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

    public static void AddTag(this NetworkEntity entity, IEntityTag tag)
    {
        var key = GetTagIndex(entity, tag);

        Tags[key] = tag;
    }
    
    public static void AddTag<T>(this NetworkEntity entity, T tag) where T : IEntityTag, new()
    {
        var key = GetTagIndex<T>(entity);

        Tags[key] = tag;
    }

    public static bool TryAddTag<T>(this NetworkEntity entity, Func<T> factory) where T : IEntityTag
    {
        var key = GetTagIndex<T>(entity);
        if (Tags.ContainsKey(key))
            return false;

        Tags[key] = factory();
        return true;
    }

    public static bool RemoveTag<T>(this NetworkEntity entity) where T : IEntityTag
    {
        var key = GetTagIndex<T>(entity);
        return Tags.Remove(key);
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

    public static bool HasTagExtending<T>(this NetworkEntity entity) where T : IAbstractEntityTag
    {
        var extensionHash = GetAbstractTagId<T>();
        if (!TagExtensionMap.TryGetValue(extensionHash, out var extending))
        {
            MelonLogger.Error($"Tag with name: {typeof(T).FullName} is not registered");
            return false;
        }

        return extending.Any(tagID =>
        {
            var key = new EntityTagIndex(entity.ID, tagID);

            return Tags.ContainsKey(key);
        });
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
            if (!TagToEntityMap.TryGetValue(tagID, out var entities))
                return Array.Empty<T>();

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

    public static T RegisterCache<T>() where T : ITagCache, new()
    {
        TypedTagCache.Register<T>();
        return TypedTagCache.Get<T>()!;
    }
    
    public static T GetCache<T>() where T : ITagCache
    {
        return TypedTagCache.Get<T>() ?? throw new Exception($"Can't find tag cache of type: {typeof(T).Name}. Did you forget to register it?");
    }

    public static void ClearAll()
    {
        Executor.RunIfHost(() =>
        {
            Tags.Clear();
        });
    }

    public static void Update(float delta)
    {
        DirectUpdateTagCache.Call(delta);
    }
    
    public static void Sync(IEntityTag entityTag)
    {
        Executor.RunIfHost(() =>
        {
            var index = entityTag.GetIndex();

            Tags.Sync(index);
        });
    }
}