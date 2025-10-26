using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Entities.Tagging.Player;
using MashGamemodeLibrary.Phase;
using UnityEngine;

namespace Clockhunt.Game.Player;

public class PlayerHandTimerTag : PlayerTag, ITagAdded, ITagUpdate, ITagRemoved
{
    private Poolee? _timerObject;
    private TextMeshPro? _text;

    private void SpawnTimer()
    {
        if (_timerObject) return;
        
        const string timerBarcode = "Mash.ClockhuntAssets.Spawnable.HandTimer";
        var spawnable = LocalAssetSpawner.CreateSpawnable(timerBarcode);
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, poolee =>
        {
            _timerObject = poolee;
            _text = poolee.GetComponentInChildren<TextMeshPro>();
        });
    }
    
    public void OnAdded(ushort entityID)
    {
        SpawnTimer();
    }
    
    public void OnRemoval(ushort entityID)
    {
        _timerObject?.Despawn();
    }
    
    public void Update(float delta)
    {
        if (_timerObject == null)
            return;

        var activePhase = GamePhaseManager.ActivePhase;
        if (!Owner.HasRig || activePhase == null)
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
        var minutes = Mathf.FloorToInt(time / 60f);
        var seconds = Mathf.FloorToInt(time % 60f);

        _text.text = $"{minutes:D2}:{seconds:D2}";
    }
}