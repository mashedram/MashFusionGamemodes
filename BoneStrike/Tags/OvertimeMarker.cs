using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Interaction;
using UnityEngine;

namespace BoneStrike.Tags;

public class OvertimeMarker : IComponent, IComponentReady, IComponentRemoved, IComponentUpdate
{
    private const string MarkerBarcode = "Mash.BoneStrike.Spawnable.BombMarker";
    private const float BlinkTime = 1f;
    private const float BlinkInterval = 0.5f;

    private MarrowEntity? _marrowEntity;

    private Poolee? _poolee;
    private MeshRenderer? _renderer;

    private bool _isVisible;
    private float _timer;

    public void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        _marrowEntity = marrowEntity;

        var spawnable = LocalAssetSpawner.CreateSpawnable(MarkerBarcode);
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, poolee =>
        {
            _poolee = poolee;
            _renderer = poolee.GetComponentInChildren<MeshRenderer>();

            if (_renderer != null)
                _renderer.enabled = false;
        });
    }

    public void OnRemoved()
    {
        if (_poolee == null) return;

        _poolee.Despawn();
        _poolee = null;
        _renderer = null;
    }

    public void Update(float delta)
    {
        if (_marrowEntity == null)
            return;

        if (_poolee == null)
            return;

        if (_renderer == null)
            return;

        _timer += delta;

        var targetTime = _isVisible ? BlinkInterval : BlinkTime;
        if (_timer < targetTime)
            return;

        _timer = 0f;
        _isVisible = !_isVisible;
        _renderer.enabled = _isVisible;
        _poolee.transform.position = _marrowEntity.transform.position;
    }
}