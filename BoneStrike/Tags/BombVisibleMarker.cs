using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace BoneStrike.Tags;

public class BombVisibleMarker : EntityTag, ITagAdded, ITagUpdate, ITagRemoved
{
    private const string MarkerBarcode = "Mash.BoneStrike.Spawnable.BombMarker";
    private const float MarkerSeconds = 10;

    private const float MinDistanceSquare = 1000f;
    private const float MaxDistanceSquare = 10000000f;

    private const float MinSize = 8f;
    private const float MaxSize = 35f;
    
    private float _timer;
    private Poolee? _markerObject;
    private bool _isSpawning;
    
    public void OnAdded(ushort entityID)
    {
        if (_markerObject != null || _isSpawning) return;
        
        if (!NetworkEntityManager.IDManager.RegisteredEntities.IDEntityLookup.TryGetValue(entityID, out var networkEntity))
            return;

        var marrow = networkEntity.GetExtender<IMarrowEntityExtender>()?.MarrowEntity;
        if (marrow == null)
            return;

        var position = marrow.transform.position;
        
        _isSpawning = true;
        var spawnable = LocalAssetSpawner.CreateSpawnable(MarkerBarcode);
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, position, Quaternion.identity, poolee =>
        {
            _markerObject = poolee;
            _isSpawning = false;
            _timer = 0f;

            var localPosition = BoneStrike.Context.LocalPlayer.RigRefs.Head.position;
            var distanceSquared = (position - localPosition).sqrMagnitude;

            var range = MathUtil.InverseLerp(MinDistanceSquare, MaxDistanceSquare, distanceSquared).Clamp01();
            var size = MathUtil.Lerp(MinSize, MaxSize, range);

            poolee.transform.GetChild(0).localScale = new Vector3(size, size, size);
        });
    }
    
    public void Update(float delta)
    {
        if (_timer > MarkerSeconds) 
            return;

        _timer += delta;
        
        if (_timer <= MarkerSeconds) 
            return;
        
        _markerObject?.Despawn();
        _markerObject = null;
    }
    
    public void OnRemoval(ushort entityID)
    {
        _markerObject?.Despawn();
        _markerObject = null;
    }
}