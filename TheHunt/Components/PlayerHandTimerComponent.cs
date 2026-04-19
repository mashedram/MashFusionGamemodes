using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.Queries;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Helpers;
using TheHunt.Phase;
using UnityEngine;

namespace TheHunt.Components;


[LocalOnly]
public class PlayerHandTimerComponent : IComponentPlayerReady, IComponentUpdate, IComponentRemoved
{
    public static readonly CachedQuery<PlayerHandTimerComponent> Query = CachedQueryManager.Create<PlayerHandTimerComponent>();

    private Transform? _compasPointer;
    private bool _isSpawning;

    private NetworkPlayer _owner = null!;
    private Poolee? _timerObject;
    private TextMeshPro? _text;

    public void OnReady(NetworkPlayer networkPlayer, MarrowEntity marrowEntity)
    {
        _owner = networkPlayer;
        SpawnTimer();
    }

    public void OnRemoved()
    {
        _timerObject?.Despawn();
        _timerObject = null;
    }

    public void Update(float delta)
    {
        if (_timerObject == null)
            return;

        var activePhase = GamePhaseManager.ActivePhase;
        // If the owner of this tag is spectating and not me, hide it
        if (!_owner.PlayerID.IsMe && _owner.IsSpectating() || activePhase == null)
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

        if (_text == null)
            return;

        var time = activePhase.Duration - activePhase.ElapsedTime;
        
        // Little edge case
        if (Gamemode.TheHunt.Config.FinallyAlwaysPlays && GamePhaseManager.ActivePhase is HuntPhase)
            time += Gamemode.TheHunt.Config.FinallyDuration;
        
        var minutes = Math.Max(Mathf.FloorToInt(time / 60f), 0);
        var seconds = Math.Max(Mathf.FloorToInt(time % 60f), 0);

        _text.text = $"{minutes:D2}:{seconds:D2}";
    }

    private void SpawnTimer()
    {
        if (_timerObject != null || _isSpawning) return;

        _isSpawning = true;
        const string timerBarcode = "Mash.TheHuntAssets.Spawnable.HandTimer";
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