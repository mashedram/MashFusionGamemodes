using System.Diagnostics.CodeAnalysis;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Tagging.Tags;

namespace MashGamemodeLibrary.Entities.Tagging;

public class TrackedEntity
{
    private readonly NetworkEntityReference _entityReference;
    private readonly Dictionary<ulong, IEntityTag> _tags = new();
    
    public TrackedEntity(NetworkEntityReference entityReference)
    {
        _entityReference = entityReference;
    }
    
    public TrackedEntity(ushort networkId)
    {
        _entityReference = new NetworkEntityReference(networkId);
    }
    
    public bool TryGetEntity([MaybeNullWhen(returnValue: false)] out NetworkEntity entity)
    {
        return _entityReference.TryGetEntity(out entity);
    }

    public ushort GetId()
    {
        return _entityReference.ID;
    }

    public void SetTag<T>(T tag) where T : IEntityTag
    {
        var tagId = EntityTagManager.GetTagId<T>();
        _tags[tagId] = tag;
    }
    
    public bool HasTag<T>() where T : IEntityTag
    {
        var tagId = EntityTagManager.GetTagId<T>();
        return _tags.ContainsKey(tagId);
    }
    
    public bool TryGetTag<T>([MaybeNullWhen(returnValue: false)] out T value) where T : IEntityTag
    {
        var tagId = EntityTagManager.GetTagId<T>();
        if (_tags.TryGetValue(tagId, out var tag)) {;
            value = (T)tag;
            return true;
        }
        
        value = default!;
        return false;
    }

    public List<T> GetTagsExtending<T>() where T: IEntityTag
    {
        var list = new List<T>();
        foreach (var tag in _tags.Values)
        {
            if (tag is T tTag)
                list.Add(tTag);
        }

        return list;
    }
}