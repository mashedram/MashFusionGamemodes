using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Audio.Players.Callers;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Gamemode;
using TheHunt.Teams;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheHunt.Nightmare.Ability.Active;

[LocalOnly]
internal class ScreamMarker : IComponent, IPlayerAttached, IRemoved, IUpdate
{
    private const string MarkerBarcode = "Mash.TheHuntAssets.Spawnable.PlayerMarker";

    private NetworkPlayer? _target;
    private Poolee? _poolee;
    private float _timer = 10f;
    
    public void OnReady(NetworkPlayer networkPlayer)
    {
        _target = networkPlayer;
        
        var spawnable = LocalAssetSpawner.CreateSpawnable(MarkerBarcode);
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, poolee =>
        {
            _poolee = poolee;
            
            if (!networkPlayer.HasRig)
                return;

            var head = networkPlayer.RigRefs.RigManager.physicsRig.m_head;
            var offset = Random.insideUnitSphere *10f;
            _poolee.transform.position = head.position + offset;
        });
    }
    
    public void OnRemoved()
    {
        if (_poolee == null) return;

        _poolee.Despawn();
        _poolee = null;
    }

    public void Update(float delta)
    {
        _timer -= delta;
        
        if (_timer > 0f)
            return;
        
        _target?.RemoveComponent<ScreamMarker>();
    }
}

public class RoarRequest : IParameterPacket<Vector3>
{
    public Vector3 RoarPosition;

    public Vector3 Value
    {
        get => RoarPosition;
        init => RoarPosition = value;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref RoarPosition);
    }
}

public class ScreamAbility : IActiveAbility, INetSerializable
{
    private float _cooldown;
    private Handedness _handedness;
    public Handedness Handedness => _handedness;

    public string Description => "Scream to mark enemies";

    public ScreamAbility()
    {
        _handedness = Handedness.RIGHT;
        _cooldown = 30f;
    }
    
    public ScreamAbility(Handedness handedness, float cooldown)
    {
        _handedness = handedness;
        _cooldown = cooldown;
    }
    
    public void UseAbility(Nightmare nightmare, NetworkPlayer networkPlayer)
    {
        if (!networkPlayer.HasRig)
            return;
        
        if (!networkPlayer.PlayerID.IsMe)
            return;

        var targets = NetworkPlayer.Players
            .Where(p => p.PlayerID.IsValid && !p.IsSpectating() && p.PlayerID.IsTeam<HiderTeam>());

        foreach (var target in targets)
        {
            target?.TryAddComponent(() => new ScreamMarker());
        } 

        var position = networkPlayer.RigRefs.Head.position;
        TheHuntContext.RoarAudioPlayer.PlayRandom(position);
    }
    public float Cooldown => _cooldown;

    public void OnAdded(NetworkPlayer networkPlayer)
    {
        // NO-OP
    }
    
    public void OnRemoved(NetworkPlayer networkPlayer)
    {
        // NO-OP
    }
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref _handedness);
        serializer.SerializeValue(ref _cooldown);
    }
}