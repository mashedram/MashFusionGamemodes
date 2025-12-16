using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Entities.Tagging.Player;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Spectating;
using UnityEngine;

namespace BoneStrike.Tags;

public class PlayerHandTimerTag : PlayerTag, ITagAdded, ITagUpdate, ITagRemoved
{
    private Poolee? _timerObject;
    private TextMeshPro? _text;
    private bool _isSpawning;

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

            _isSpawning = false;
        });
    }
    
    public void OnAdded(ushort entityID)
    {
        SpawnTimer();
    }
    
    public void OnRemoval(ushort entityID)
    {
        _timerObject?.Despawn();
        _timerObject = null;
    }
    
    public void Update(float delta)
    {
        if (_timerObject == null)
            return;

        var activePhase = GamePhaseManager.ActivePhase;
        if (!Owner.HasRig || activePhase == null || Owner.PlayerID.IsSpectatingAndHidden())
        {
            _timerObject.gameObject.SetActive(false);
            return;
        }
        
        var leftHand = Owner.RigRefs.LeftHand.transform;
        var position = leftHand.position + leftHand.forward * 0.05f + leftHand.right * -0.05f;
        var rotation = leftHand.rotation;
        
        _timerObject.transform.SetPositionAndRotation(position, rotation);

        if (_text == null)
            return;
        
        var time = activePhase.Duration - activePhase.ElapsedTime;
        var minutes = Math.Max(Mathf.FloorToInt(time / 60f), 0);
        var seconds = Math.Max(Mathf.FloorToInt(time % 60f), 0);

        _text.text = $"{minutes:D2}:{seconds:D2}";
    }
}