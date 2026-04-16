using System.Globalization;
using BoneStrike.Phase;
using BoneStrike.Teams;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using Il2CppTMPro;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.ECS.Networking;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Actions;
using MashGamemodeLibrary.Player.Team;
using MelonLoader;
using UnityEngine;

namespace BoneStrike.Tags;

public class DefusableTag : IComponentReady, IGrabPredicate, IComponentRemoved, IComponentUpdate, IGrabCallback, IDropCallback, INetworkEvents,
    INetSerializable
{
    private static readonly RemoteEvent DefuseEvent = new("DefuseEvent", OnDefuse, CommonNetworkRoutes.AllToHost);
    private const int SyncTimeEventIndex = 0;

    private PlayerID? _grabber;
    private Transform? _offset;
    private TextMeshPro? _text;
    private Poolee? _timerObject;

    private TextMeshPro? _clockText;

    private bool _isDefused;
    private float _timer;

    public void OnDropped(GrabData grab)
    {
        _grabber = null;
        _timerObject?.gameObject.SetActive(false);

        // We just enable locomotion here for all, to prevent any funny desyncs
        if (grab.NetworkPlayer != null && GamePhaseManager.IsPhase<DefusePhase>())
        {
            LocalControls.LockedMovement = false;
        }

        if (grab.NetworkPlayer != null && grab.NetworkPlayer.PlayerID.IsMe)
            this.SendEvent(SyncTimeEventIndex, sizeof(float), writer => writer.Write(_timer));

    }

    public void OnGrabbed(GrabData grab)
    {
        if (grab.NetworkPlayer == null)
            return;
        if (GamePhaseManager.IsPhase<PlantPhase>())
            return;

        Executor.RunIfMe(grab.NetworkPlayer.PlayerID, () =>
        {
            LocalControls.LockedMovement = true;
        });

        _grabber = grab.NetworkPlayer!.PlayerID;
        _timerObject?.gameObject.SetActive(true);

        if (grab.NetworkPlayer != null && grab.NetworkPlayer.PlayerID.IsMe)
            this.SendEvent(SyncTimeEventIndex, sizeof(float), writer => writer.Write(_timer));
    }

    public bool CanGrab(GrabData grab)
    {
        if (GamePhaseManager.IsPhase<PlantPhase>())
        {
            return LogicTeamManager.IsLocalTeam<TerroristTeam>();
        }

        return LogicTeamManager.IsLocalTeam<CounterTerroristTeam>();
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _timer);
    }

    public void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        SpawnTimer(marrowEntity.transform);
        
        _clockText = marrowEntity.GetComponentInChildren<TextMeshPro>();
    }

    public void OnRemoved(NetworkEntity networkEntity)
    {
        _timerObject?.Despawn();
    }

    public void Update(float delta)
    {
        var activePhase = GamePhaseManager.ActivePhase;
        if (activePhase != null && _clockText != null)
        {
            var time = activePhase.Duration - activePhase.ElapsedTime;
            var minutes = Math.Max(Mathf.FloorToInt(time / 60f), 0);
            var seconds = Math.Max(Mathf.FloorToInt(time % 60f), 0);

            _clockText.text = $"{minutes:D2}:{seconds:D2}";
        }
        
        if (_grabber == null) return;
        if (GamePhaseManager.IsPhase<PlantPhase>()) return;

        _timer += delta;

        var timeout = BoneStrike.Config.DefuseTime;
        var remainingTime = timeout - _timer;
        if (remainingTime <= 0f && !_isDefused)
        {
            // Prevent it from triggering again
            Defuse();
            return;
        }

        if (_offset != null && _timerObject != null)
        {
            _offset.position = _timerObject.transform.position + new Vector3(0f, 0.2f, 0f);
        }

        if (_text == null) return;

        _text.text = (timeout - _timer).ToString("F1", CultureInfo.InvariantCulture);
    }

    // Helpers

    private void Defuse()
    {
        if (_grabber == null)
            return;
        if (!_grabber.IsMe)
            return;
        if (_isDefused)
            return;

        _isDefused = true;

        if (NetworkInfo.IsHost)
        { 
            PlayerStatisticsTracker.Increment(BonestrikeStatisticsKeys.Defusals);
            WinManager.Win<CounterTerroristTeam>();
        }
        else
        {
            MelonLogger.Msg("Sent defuse request.");
            DefuseEvent.CallFor(PlayerIDManager.GetHostID());
        }
        _grabber = null;
    }

    private void SpawnTimer(Transform parent)
    {
        if (_timerObject) return;

        const string timerBarcode = "Mash.BoneStrike.Spawnable.Timer";
        var spawnable = LocalAssetSpawner.CreateSpawnable(timerBarcode);
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, poolee =>
        {
            poolee.gameObject.SetActive(false);

            _timerObject = poolee;
            _offset = _timerObject.transform.GetChild(0);
            _text = poolee.GetComponentInChildren<TextMeshPro>();

            poolee.transform.parent = parent;
            poolee.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        });
    }

    public void OnEvent(byte senderId, byte eventIndex, NetReader reader)
    {
        switch (eventIndex)
        {
            case SyncTimeEventIndex:
            {
                if (PlayerIDManager.LocalID == senderId)
                    return;

                _timer = reader.ReadSingle();
                break;
            }
            // The old defuse system should be fixed
        }
    }

    private static void OnDefuse(byte senderId)
    {
        if (GamePhaseManager.IsPhase<PlantPhase>())
            return;

        if (!PlayerIDManager.SmallIDLookup.TryGetValue(senderId, out var playerID))
            return;
        
        if (!playerID.IsValid)
            return;

        if (!playerID.IsTeam<CounterTerroristTeam>())
        {
            MelonLogger.Warning($"Player {playerID} attempted to defuse but is not on the Counter-Terrorist team.");
        }

        PlayerStatisticsTracker.Increment(BonestrikeStatisticsKeys.Defusals);
        Executor.RunIfHost(WinManager.Win<CounterTerroristTeam>);
    }
}