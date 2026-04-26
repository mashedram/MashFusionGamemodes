using System.Collections;
using LabFusion.Entities;
using LabFusion.Scene;
using MashGamemodeLibrary.Entities.Association.Impl;
using MashGamemodeLibrary.Entities.Behaviour;
using MashGamemodeLibrary.Entities.Behaviour.Cache;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Entities.Interaction.Grabbing;

[RequireStaticConstructor]
public static class PlayerGrabManager
{
    public delegate bool GrabPredicateHanlder(GrabRequest request);
    public static GrabPredicateHanlder? GrabPredicate;
    // Caches

    private static readonly IAssociatedBehaviourCache<NetworkEntityAssociation, IGrabCallback> GrabCallbackCache = BehaviourManager.CreateCache<NetworkEntityAssociation, IGrabCallback>();
    private static readonly IAssociatedBehaviourCache<NetworkEntityAssociation, IDropCallback> DropCallbackCache = BehaviourManager.CreateCache<NetworkEntityAssociation, IDropCallback>();
    private static readonly IAssociatedBehaviourCache<NetworkEntityAssociation, IGrabPredicate> GrabPredicateCache = BehaviourManager.CreateCache<NetworkEntityAssociation, IGrabPredicate>();
    
    // Methods
    
    public static bool CanGrab(this GrabRequest grabRequest)
    {
        if (!grabRequest.IsValid)
            return true;
        
        // Always allow other clients to grab things they request
        if (grabRequest.NetworkPlayer.PlayerID is not { IsMe: true })
            return true;

        var networkEntity = grabRequest.GrabbedNetworkEntity;
        if (networkEntity == null)
            return true;
        
        // Check local spectator
        if (SpectatorExtender.IsLocalPlayerSpectating())
            return false;
        
        // Check spectator
        var targetPlayer = grabRequest.GrabbedNetworkEntity?.GetExtender<NetworkPlayer>();
        if (targetPlayer != null && targetPlayer.PlayerID.IsSpectating())
            return false;
        
        // Check the global predicate
        if (GrabPredicate != null)
        {
            var result = Executor.Try(() => GrabPredicate(grabRequest), false);
            if (!result) return false;
        }
        
        // Check predicate cache on the entity
        var predicates = GrabPredicateCache
            .GetAll(networkEntity.ID);
        
        return predicates.All(predicate => predicate.Try(p => p.CanGrab(grabRequest), false));
    }
    
    public static void OnDrop(GrabRequest grab)
    {
        if (!grab.IsValid)
            return;
        if (grab.GrabbedNetworkEntity == null)
            return;
        if (grab.GrabbedHost == null)
            return;
        
        // Ensure that there is not another hand still grabbing the object, as the OnDrop will be called before the hand is detached
        if (grab.GrabbedHost.HandCount() > 0)
            return;
        
        DropCallbackCache.ForEach(grab.GrabbedNetworkEntity.ID, callback => callback.Try(c => c.OnDropped(grab)));
    }
    
    public static void OnGrab(GrabRequest grab)
    {
        if (!grab.IsValid)
            return;
        if (grab.GrabbedNetworkEntity == null)
            return;
        if (grab.GrabbedHost == null)
            return;
        
        // Check against one here, because the OnGrab will be called after the hand is attached
        if (grab.GrabbedHost.HandCount() > 1)
            return;
        
        GrabCallbackCache.ForEach(grab.GrabbedNetworkEntity.ID, callback => callback.Try(c => c.OnGrabbed(grab)));
    }
}