using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Extensions;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;

public static class PhysicsRigCache
{
    private static readonly Dictionary<PhysicsRig, CachedPhysicsRig> Cache = new(new UnityComparer());
    
    public static void OnPhysicsRigCreated(MarrowEntity marrowEntity)
    {
        var physicsRig = marrowEntity.GetComponent<PhysicsRig>();
        if (physicsRig == null)
            return;
        
        Cache[physicsRig] = new CachedPhysicsRig(physicsRig);
    }
    
    public static void OnPhysicsRigDestroyed(MarrowEntity marrowEntity)
    {
        var physicsRig = marrowEntity.GetComponent<PhysicsRig>();
        if (physicsRig == null)
            return;
        
        Cache.Remove(physicsRig);
    }
    
    public static void Reset()
    {
        Cache.Clear();
    }
    
    // Accessors
    
    public static IEnumerable<CachedPhysicsRig> GetIgnoringCollisions()
    {
        return Cache.Values.Where(cachedRig => !cachedRig.IsColliding);
    }
    
    public static IEnumerable<CachedPhysicsRig> GetAll()
    {
        return Cache.Values;
    }
    
    public static CachedPhysicsRig? GetRig(PhysicsRig cachedPhysicsRig)
    {
        return Cache.GetValueOrDefault(cachedPhysicsRig);
    }
}