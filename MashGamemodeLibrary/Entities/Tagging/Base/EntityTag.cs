using LabFusion.Entities;
using UnityEngine;

namespace MashGamemodeLibrary.Entities.Tagging.Base;

public class EntityTag : IEntityTag, ITagAddedInternal
{
    private readonly double _createdAt = Time.timeSinceLevelLoadAsDouble;
    private NetworkEntity? _entity;
    private NetworkEntityReference _entityID;
    private bool _hasLoaded;
    private EntityTagIndex _tagIndex;
    protected NetworkEntity Entity => GetEntity();

    public EntityTagIndex GetIndex()
    {
        return _tagIndex;
    }

    public double CreatedAt()
    {
        return _createdAt;
    }

    public bool HasLoaded()
    {
        return _hasLoaded;
    }

    public void OnAddInternal(EntityTagIndex tag)
    {
        _entityID = new NetworkEntityReference(tag.EntityID);
        _tagIndex = tag;
        _hasLoaded = true;
    }

    private NetworkEntity GetEntity()
    {
        if (_entity != null) return _entity;

        _entity = _entityID.GetEntity();
        return _entity;
    }

    protected void Sync()
    {
        EntityTagManager.Sync(this);
    }
}