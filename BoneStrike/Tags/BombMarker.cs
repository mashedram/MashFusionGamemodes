using BoneStrike.Phase;
using BoneStrike.Teams;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.ECS.Query;
using MashGamemodeLibrary.Phase;
using UnityEngine;

namespace BoneStrike.Tags;

public class BombMarker : IComponent, IComponentReady, IComponentRemoved, IPhaseChanged
{
    public static readonly CachedQuery<BombMarker> Query = EcsManager.CacheQuery<BombMarker>();
    
    private const float MaxVelocitySquared = 1000f;

    private MarrowEntity? _marrowEntity;
    private Vector3? _returnPosition;
    private List<Rigidbody>? _rigidbodies;

    public void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        _marrowEntity = marrowEntity;
        _rigidbodies = _marrowEntity._bodies.Select(b => b._rigidbody).ToList();
    }

    public void OnPhaseChange(GamePhase gamePhase)
    {
        if (_marrowEntity == null)
            return;

        if (gamePhase is DefusePhase)
        {
            _returnPosition = _marrowEntity.transform.position;
        }
    }

    public void OnRemoved(NetworkEntity networkEntity)
    {
        WinManager.Win<CounterTerroristTeam>();
    }
    
    public void Update(float delta)
    {
        if (_rigidbodies == null)
            return;

        var squaredVelocity = _rigidbodies.Average(r => r.velocity.sqrMagnitude);
        if (squaredVelocity < MaxVelocitySquared)
            return;

        _rigidbodies.ForEach(r => r.velocity = Vector3.zero);
        if (_returnPosition.HasValue)
            // We can be sure marrowentity exists because otherwise rigidbodies would be null
            _marrowEntity!.transform.position = _returnPosition.Value;
    }
}