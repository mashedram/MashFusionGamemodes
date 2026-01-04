using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Query;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Spectating;
using UnityEngine;

namespace BoneStrike.Tags;

[LocalOnly]
public class PlayerHandTimerTag : IComponentPlayerReady, IComponentUpdate, IComponentRemoved
{
    public static readonly CachedQuery<PlayerHandTimerTag> Query = EcsManager.CacheQuery<PlayerHandTimerTag>();
    
    private Transform? _compasPointer;
    private bool _isSpawning;

    private NetworkPlayer _owner = null!;

    // TODO: Make tags a target and get the closest one
    public GameObject? Target = null;
    private TextMeshPro? _text;
    private Poolee? _timerObject;

    public void OnReady(NetworkPlayer networkPlayer, MarrowEntity marrowEntity)
    {
        _owner = networkPlayer;
        SpawnTimer();
    }

    public void OnRemoved(NetworkEntity networkEntity)
    {
        _timerObject?.Despawn();
        _timerObject = null;
    }

    public void Update(float delta)
    {
        if (_timerObject == null)
            return;

        var activePhase = GamePhaseManager.ActivePhase;
        if (!_owner.HasRig || activePhase == null || _owner.PlayerID.IsHidden())
        {
            _timerObject.gameObject.SetActive(false);
            return;
        }
        _timerObject.gameObject.SetActive(true);

        var leftHand = _owner.RigRefs.LeftHand.transform;
        var leftHandPosition = leftHand.position;
        var position = leftHandPosition + leftHand.forward * 0.05f + leftHand.right * -0.05f;
        var rotation = leftHand.rotation;

        _timerObject.transform.SetPositionAndRotation(position, rotation);

        if (_compasPointer != null)
        {
            var hasTarget = Target != null;

            _compasPointer.gameObject.SetActive(hasTarget);
            if (hasTarget)
            {
                var direction = (Target!.transform.position - position).normalized;
                var projectedDirection = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
                var directionAngle = Mathf.Atan2(projectedDirection.x, projectedDirection.z) * Mathf.Rad2Deg;

                var right = leftHand.right;
                var upDirection = Vector3.ProjectOnPlane((-right + leftHand.up) * 2.5f, Vector3.up).normalized;
                var compassOffsetAngle = Mathf.Atan2(upDirection.x, upDirection.z) * Mathf.Rad2Deg;

                var isFlipped = (-right).y > 0f;

                var offset = isFlipped ? 180f : 0f;
                var finalAngle = -directionAngle + compassOffsetAngle + offset;
                _compasPointer.localEulerAngles = new Vector3(finalAngle, 0f, 0f);
            }
        }

        if (_text == null)
            return;

        var time = activePhase.Duration - activePhase.ElapsedTime;
        var minutes = Math.Max(Mathf.FloorToInt(time / 60f), 0);
        var seconds = Math.Max(Mathf.FloorToInt(time % 60f), 0);

        _text.text = $"{minutes:D2}:{seconds:D2}";
    }

    private void SpawnTimer()
    {
        if (_timerObject != null || _isSpawning) return;

        _isSpawning = true;
        const string timerBarcode = "Mash.BoneStrike.Spawnable.HandTimer";
        var spawnable = LocalAssetSpawner.CreateSpawnable(timerBarcode);
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, poolee =>
        {
            _timerObject = poolee;
            _text = poolee.GetComponentInChildren<TextMeshPro>();
            _compasPointer = poolee.transform.FindChild("Compas");

            _isSpawning = false;
        });
    }
}