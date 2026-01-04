using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Spectating;
using UnityEngine;

namespace Clockhunt.Game.Player;

public class PlayerHandTimerTag : IComponentPlayerReady, IComponentUpdate, IComponentRemoved
{
    private NetworkPlayer _owner = null!;
    private Poolee? _timerObject;
    private TextMeshPro? _text;
    private bool _isSpawning;

    private void SpawnTimer()
    {
        if (_timerObject != null || _isSpawning) return;
        
        _isSpawning = true;
        const string timerBarcode = "Mash.ClockhuntAssets.Spawnable.HandTimer";
        var spawnable = LocalAssetSpawner.CreateSpawnable(timerBarcode);
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, poolee =>
        {
            _timerObject = poolee;
            _text = poolee.GetComponentInChildren<TextMeshPro>();

            _isSpawning = false;
        });
    }

    public void OnReady(NetworkPlayer networkPlayer, MarrowEntity marrowEntity)
    {
        _owner = networkPlayer;
        SpawnTimer();
        
    }
    
    public void OnRemoved(NetworkEntity networkEntity)
    {
        _timerObject?.Despawn();
    }
    
    public void Update(float delta)
    {
        if (_timerObject == null)
            return;

        var activePhase = GamePhaseManager.ActivePhase;
        if (!_owner.HasRig || activePhase == null || _owner.PlayerID.IsSpectatingAndHidden())
        {
            _timerObject.gameObject.SetActive(false);
            return;
        }
        
        var leftHand = _owner.RigRefs.LeftHand.transform;
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