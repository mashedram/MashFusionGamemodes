using LabFusion.Entities;
using UnityEngine;

namespace MashGamemodeLibrary.Entities.Tagging.Base;

public class EntityTag : IEntityTag, ITagAddedInternal
{
    private EntityTagIndex _tagIndex;
    private NetworkEntity? _entity;
    private NetworkEntityReference _entityID;
    protected NetworkEntity Entity => GetEntity();

    public void OnAddInternal(EntityTagIndex tag)
    {
        _entityID = new NetworkEntityReference(tag.EntityID);
        _tagIndex = tag;
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
    
    protected void Sync()
    {
        EntityTagManager.Sync(this);
    }
}