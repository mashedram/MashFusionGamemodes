using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Spectating.data.Colliders;
using MashGamemodeLibrary.Player.Spectating.Data.Components.Colliders.Caches;

namespace MashGamemodeLibrary.Player.Data.Components.Colliders.Caches;

public static class CachedColliderCache
{
    private static readonly Dictionary<PhysicsRig, CachedPhysicsRig> PhysicsRigCache = new();
    private static readonly Dictionary<MarrowEntity, CachedMarrowEntity> PropCache = new();

    public static void OnInitializeMelon()
    {
    }
    
    private static void OnPhysicsRigCreated(PhysicsRig physicsRig)
    {
        var cachedPhysicsRig = new CachedPhysicsRig(physicsRig);
        PhysicsRigCache.Add(physicsRig, cachedPhysicsRig);
        
        foreach (var cachedMarrowEntity in PropCache.Values)
        {
            cachedPhysicsRig.OnColliderCached(cachedMarrowEntity);
        }
    }
    
    private static void OnPhysicsRigDestroyed(PhysicsRig physicsRig)
    {
        PhysicsRigCache.Remove(physicsRig);
    }

    public static void OnMarrowEntityCreated(MarrowEntity marrowEntity)
    {
        if (marrowEntity.name == "PhysicsRig")
        {
            var physicsRig = marrowEntity.GetComponent<PhysicsRig>();
            if (physicsRig != null)
            {
                OnPhysicsRigCreated(physicsRig);
                return;
            }
        }
        
        var cachedMarrowEntity = new CachedMarrowEntity(marrowEntity);
        PropCache.Add(marrowEntity, cachedMarrowEntity);
        foreach (var cachedRig in PhysicsRigCache.Values)
        {
            cachedRig.OnColliderCached(cachedMarrowEntity);
        }
    }
    
    public static void OnMarrowEntityDestroyed(MarrowEntity marrowEntity)
    {
        if (marrowEntity.name == "PhysicsRig")
        {
            var physicsRig = marrowEntity.GetComponent<PhysicsRig>();
            if (physicsRig != null)
            {
                OnPhysicsRigDestroyed(physicsRig);
                return;
            }
        }

        PropCache.Remove(marrowEntity);
    }
    
    // Accessors
    public static IEnumerable<ICachedCollider> CachedEntities => (PropCache.Values as IEnumerable<ICachedCollider>).Union(PhysicsRigCache.Values);

    public static CachedPhysicsRig? GetPlayerCollider(byte localSmallID)
    {
        if (!NetworkPlayerManager.TryGetPlayer(localSmallID, out var networkPlayer))
            return null;
        
        if (!networkPlayer.HasRig)
            return null;

        var physicsRig = networkPlayer.RigRefs.RigManager.physicsRig;
        return PhysicsRigCache.GetValueOrDefault(physicsRig);
    }
}