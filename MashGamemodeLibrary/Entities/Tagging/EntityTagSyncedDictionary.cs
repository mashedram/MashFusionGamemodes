using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Entities.Tagging;

public readonly record struct EntityTagIndex(ushort EntityID, ulong TagID)
{
    public readonly ushort EntityID = EntityID;
    public readonly ulong TagID = TagID;
}

public class EntityTagSyncedDictionary : SyncedDictionary<EntityTagIndex, IEntityTag>
{
    // Registry Shenanigans
    private static readonly Dictionary<ulong, Func<IEntityTag>> TagFactories = new();

    private static ulong GetTagId<T>() where T : IEntityTag
    {
        return typeof(T).FullName?.GetStableHash() ?? throw new Exception("Failed to get hash code for tag type: " + typeof(T).FullName);
    }
    
    public static void Register<T>() where T : IEntityTag, new()
    {
        var tagId = GetTagId<T>();
#if DEBUG
        var fullname = typeof(T).FullName!;
        MelonLogger.Msg($"Registering type ${fullname} with hash {fullname.GetHashCode()} or {tagId}");
#endif
        TagFactories[tagId] = () => new T();
    }
    
    // Actual implementation
    
    public EntityTagSyncedDictionary(string name) : base(name)
    {
        
    }

    protected override int? GetSize(DictionaryEdit<EntityTagIndex, IEntityTag> data)
    {
        return sizeof(ushort) + sizeof(ulong) +
               (data.Value is INetSerializable serializable ? serializable.GetSize() : 0);
    }

    protected override void WriteKey(NetWriter writer, EntityTagIndex key)
    {
        writer.Write(key.EntityID);
        writer.Write(key.TagID);
    }

    protected override EntityTagIndex ReadKey(NetReader reader)
    {
        var entityID = reader.ReadUInt16();
        var tagID = reader.ReadUInt64();

        return new EntityTagIndex(entityID, tagID);
    }

    protected override void WriteValue(NetWriter writer, IEntityTag value)
    {
        if (value is INetSerializable serializable)
        {
            serializable.Serialize(writer);
        }
    }

    protected override IEntityTag ReadValue(NetReader reader, EntityTagIndex key)
    {
        #if DEBUG
        // ReSharper disable once InvertIf
        if (!TagFactories.ContainsKey(key.TagID))
        {
            MelonLogger.Error($"Invalid tag received with id: {key.TagID} on entity: {key.EntityID}");
            throw new Exception($"Invalid tag received with id: {key.TagID} on entity: {key.EntityID}");
        }
        #endif

        var tag = TagFactories[key.TagID].Invoke();

        if (tag is INetSerializable serializable)
        {
            serializable.Serialize(reader);
        }

        return tag;
    }
}