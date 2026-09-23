using Il2CppSLZ.Marrow.Pool;
using LabFusion.Entities;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Entities.ECS.Attributes;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Nightmare.Ability.Active;
using TheHunt.Teams;
using UnityEngine;
using Random = System.Random;

namespace TheHunt.Nightmare.Ability.Passive;


[LocalOnly]
internal class TrackMarker : IComponent, IPlayerAttached, IRemoved, IUpdate
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
            _poolee.transform.position = head.position;
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
        if (_poolee == null || _target is not { HasRig: true })
            return;
        
        _timer -= delta;
        if (_timer > 0f)
            return;

        var head = _target.RigRefs.Head.position;
        _poolee.transform.position = head;
    }
}

public class TrackAbility : IAbility, IPlayerAttached, IRemoved
{

    public void OnReady(NetworkPlayer networkPlayer)
    {
        var targets = NetworkPlayer.Players
            .Where(p => p.PlayerID.IsValid && !p.IsSpectating() && p.PlayerID.IsTeam<HiderTeam>());

        foreach (var target in targets)
        {
            target?.TryAddComponent(() => new TrackMarker());
        } 
    }
    public void OnRemoved()
    {
        foreach (var networkPlayer in NetworkPlayer.Players)
        {
            networkPlayer.RemoveComponent<TrackMarker>();
        }
    }
}