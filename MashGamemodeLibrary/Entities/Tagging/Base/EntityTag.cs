using LabFusion.Entities;
using UnityEngine;

namespace MashGamemodeLibrary.Entities.Tagging.Base;

public class EntityTag : IEntityTag, ITagAddedInternal
{
    private EntityTagIndex _tagIndex;
    private NetworkEntity? _entity;
    private NetworkEntityReference _entityID;
    private double _createdAt = Time.timeSinceLevelLoadAsDouble;
    private bool _hasLoaded;
    protected NetworkEntity Entity => GetEntity();

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

    protected void Sync()
    {
        EntityTagManager.Sync(this);
    }
}