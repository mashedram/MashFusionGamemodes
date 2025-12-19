using System.Globalization;
using BoneStrike.Phase;
using BoneStrike.Teams;
using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Components;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;

namespace BoneStrike.Tags;

public class DefusableTag : EntityTag, IEntityGrabPredicate, ITagRemoved, ITagUpdate, IEntityGrabCallback, IEntityDropCallback, INetSerializable
{

    private bool _isGrabbed;
    private Transform? _offset;
    private TextMeshPro? _text;
    private float _timer;
    private Poolee? _timerObject;

    public void OnDrop(GrabData grab)
    {
        _isGrabbed = false;
        _timerObject?.gameObject.SetActive(false);

        // We just enable locomotion here for all, to prevent any funny desyncs
        if (grab.NetworkPlayer != null && GamePhaseManager.IsPhase<DefusePhase>())
        {
            LocalControls.LockedMovement = false;
        }

        Sync();
    }

    public void OnGrab(GrabData grab)
    {
        if (GamePhaseManager.IsPhase<PlantPhase>()) return;

        if (grab.NetworkPlayer != null)
        {
            Executor.RunIfMe(grab.NetworkPlayer.PlayerID, () =>
            {
                LocalControls.LockedMovement = true;
            });
        }

        _isGrabbed = true;
        SpawnTimer();
        _timerObject?.gameObject.SetActive(true);
    }

    public bool CanGrab(GrabData grab)
    {
        if (GamePhaseManager.IsPhase<PlantPhase>())
        {
            return TeamManager.IsLocalTeam<TerroristTeam>();
        }

        return TeamManager.IsLocalTeam<CounterTerroristTeam>();
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _timer);
    }

    public void OnRemoval(ushort entityID)
    {
        _timerObject?.Despawn();
    }

    public void Update(float delta)
    {
        if (!_isGrabbed) return;
        if (GamePhaseManager.IsPhase<PlantPhase>()) return;

        _timer += delta;

        var timeout = BoneStrike.Config.DefuseTime;
        if (_timer > timeout)
        {
            // Prevent it from triggering again
            _isGrabbed = false;
            WinManager.Win<CounterTerroristTeam>();
            return;
        }

        if (_offset != null && _timerObject != null)
        {
            _offset.position = _timerObject.transform.position + new Vector3(0f, 0.2f, 0f);
        }

        if (_text == null) return;

        _text.text = (timeout - _timer).ToString("F1", CultureInfo.InvariantCulture);
    }

    private void SpawnTimer()
    {
        if (_timerObject) return;

        var marrow = Entity.GetExtender<IMarrowEntityExtender>();
        if (marrow == null) return;

        var transform = marrow.MarrowEntity.transform;

        const string timerBarcode = "Mash.BoneStrike.Spawnable.Timer";
        var spawnable = LocalAssetSpawner.CreateSpawnable(timerBarcode);
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, poolee =>
        {
            _timerObject = poolee;
            _offset = _timerObject.transform.GetChild(0);
            _text = poolee.GetComponentInChildren<TextMeshPro>();

            poolee.transform.parent = transform;
            poolee.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        });
    }
}