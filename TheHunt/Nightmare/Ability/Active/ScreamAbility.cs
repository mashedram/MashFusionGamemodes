using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Loaders;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Callers;
using MashGamemodeLibrary.Audio.Players.Object;
using MashGamemodeLibrary.Audio.Registry;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Gamemode;
using TheHunt.Teams;
using UnityEngine;

namespace TheHunt.Nightmare.Ability.Active;

[LocalOnly]
internal class ScreamMarker : IComponent, IComponentPlayerReady, IComponentRemoved, IComponentUpdate
{
    private const string MarkerBarcode = "Mash.TheHuntAssets.Spawnable.PlayerMarker";

    private NetworkPlayer? _target;
    private Poolee? _poolee;
    private float _timer = 10f;
    
    public void OnReady(NetworkPlayer networkPlayer, MarrowEntity marrowEntity)
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
            _poolee.transform.position = head.position;
        });
    }
    
    public void OnRemoved(NetworkEntity networkEntity)
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
        
        Executor.RunIfHost(() =>
        {
            _target?.RemoveComponent<ScreamMarker>();
        });
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

public class ScreamAbility : IActiveAbility
{
    public Handedness Handedness => Handedness.LEFT;

    public void UseAbility(NetworkPlayer networkPlayer)
    {
        if (!networkPlayer.HasRig)
            return;
        
        if (!networkPlayer.PlayerID.IsMe)
            return;
        
        var hiders = NetworkPlayer.Players
            .Where(p => p.PlayerID.IsValid && p.PlayerID.IsTeam<HiderTeam>());

        foreach (var player in hiders)
        {
            player.TryAddComponent(() => new ScreamMarker());
        }

        var position = networkPlayer.RigRefs.Head.position;
        TheHuntContext.RoarAudioPlayer.PlayRandom(position);
    }
    public float Cooldown => 30f;

    public void OnAdded(NetworkPlayer networkPlayer)
    {
        // NO-OP
    }
    
    public void OnRemoved(NetworkPlayer networkPlayer)
    {
        // NO-OP
    }
}