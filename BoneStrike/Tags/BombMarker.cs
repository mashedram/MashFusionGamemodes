using BoneStrike.Phase;
using BoneStrike.Teams;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Queries;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;
using UnityEngine;

namespace BoneStrike.Tags;

public class BombMarker : IComponent, IComponentReady, IComponentRemoved, IPhaseChanged, IComponentUpdate, IGrabCallback, IDropCallback
{
    public static readonly CachedQuery<BombMarker> Query = CachedQueryManager.Create<BombMarker>();

    private const float MaxVelocitySquared = 1000f;

    public NetworkEntity? NetworkEntity;
    public MarrowEntity? MarrowEntity;
    private Vector3? _returnPosition;
    private List<Rigidbody>? _rigidbodies;

    private bool _isGrabbed;

    public void OnReady(NetworkEntity networkEntity, MarrowEntity marrowEntity)
    {
        NetworkEntity = networkEntity;
        MarrowEntity = marrowEntity;
        _rigidbodies = MarrowEntity._bodies.Select(b => b._rigidbody).ToList();
    }

    public void OnPhaseChange(GamePhase gamePhase)
    {
        if (MarrowEntity == null)
            return;

        if (gamePhase is DefusePhase)
        {
            _returnPosition = MarrowEntity.transform.position;
        }
    }

    public void OnRemoved(NetworkEntity networkEntity)
    {
        Executor.RunIfHost(WinManager.Win<CounterTerroristTeam>);
    }

    public void Update(float delta)
    {
        if (_rigidbodies == null)
            return;

        if (_isGrabbed)
            return;

        var squaredVelocity = _rigidbodies.Average(r => r.velocity.sqrMagnitude);
        if (squaredVelocity < MaxVelocitySquared)
            return;

        _rigidbodies.ForEach(r => r.velocity = Vector3.zero);
        if (_returnPosition.HasValue)
            // We can be sure marrowentity exists because otherwise rigidbodies would be null
            MarrowEntity!.transform.position = _returnPosition.Value;
    }

    public void OnGrabbed(GrabData grab)
    {
        _isGrabbed = true;
    }

    public void OnDropped(GrabData grab)
    {
        _isGrabbed = false;
    }
}