using System.Collections;
using System.Collections.Immutable;
using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Player.Spectating.data.Colliders;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Colliders.Caches;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;

public class CachedMarrowEntity : ICachedCollider, IHibernationEntity
{

    public CachedMarrowEntity(MarrowEntity entity)
    {
        Entity = entity;
        Bodies = entity._bodies.Select(b => new CachedMarrowBody(b)).ToImmutableArray();
    }
    public MarrowEntity Entity { get; init; }
    public ImmutableArray<CachedMarrowBody> Bodies { get; init; }
    public bool IsHibernating { get; set; }

    public IEnumerator<Collider> GetEnumerator()
    {
        return (from body in Bodies from collider in body.Colliders select collider.Collider).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void SetColliding(Collider collider, bool isColliding)
    {
        foreach (var cachedMarrowBody in Bodies)
        {
            cachedMarrowBody.SetColliding(collider, isColliding);
        }
    }

    public void SetColliding(ICachedCollider other, bool isColliding)
    {
        foreach (var cachedMarrowBody in Bodies)
        {
            cachedMarrowBody.SetColliding(other, isColliding);
        }
    }
}