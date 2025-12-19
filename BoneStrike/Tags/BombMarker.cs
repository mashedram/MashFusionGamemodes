using BoneStrike.Phase;
using BoneStrike.Teams;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Phase.Tags;
using UnityEngine;

namespace BoneStrike.Tags;

public class BombMarker : EntityTag, IMarrowLoaded, ITagRemoved, ITagUpdate, IPhaseChangedTag
{
    private const float MaxVelocitySquared = 1000f;

    private MarrowEntity? _marrowEntity;
    private Vector3? _returnPosition;
    private List<Rigidbody>? _rigidbodies;

    public void OnLoaded(NetworkEntity networkEntity, MarrowEntity marrowEntity)
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

    public void OnRemoval(ushort entityID)
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