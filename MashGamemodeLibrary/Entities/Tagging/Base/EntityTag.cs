using LabFusion.Entities;
using UnityEngine;

namespace MashGamemodeLibrary.Entities.Tagging.Base;

public class EntityTag : ITagAdded, IEntityTag
{
    private NetworkEntity? _entity;
    private NetworkEntityReference _entityID;
    private GameObject? _gameObject;
    protected NetworkEntity Entity => GetEntity();

    public void OnAdded(ushort entityID)
    {
        _entityID = new NetworkEntityReference(entityID);
    }

    private NetworkEntity GetEntity()
    {
        if (_entity != null) return _entity;

        _entity = _entityID.GetEntity();
        return _entity;
    }
}