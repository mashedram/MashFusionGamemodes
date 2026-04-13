using System.Diagnostics;
using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Scheduler;

public record struct MarrowEntityCollisionTask(CachedMarrowEntity Entity, CachedPhysicsRig PhysicsRig, bool ShouldCollide)
{
    public CachedMarrowEntity Entity { get; } = Entity;
    public CachedPhysicsRig PhysicsRig { get; } = PhysicsRig;
}

public static class MarrowEntityCollisionScheduler
{
    private const int TasksPerFrame = 64;
    private static readonly Queue<MarrowEntityCollisionTask> Tasks = new();
    
    public static void ScheduleCollisionCheck(CachedMarrowEntity entity, CachedPhysicsRig physicsRig, bool shouldCollide)
    {
        Tasks.Enqueue(new MarrowEntityCollisionTask(entity, physicsRig, shouldCollide));
    }

    public static void ScheduleForIgnoringRigs(CachedMarrowEntity entity)
    {
        foreach (var physicsRig in PhysicsRigCache.GetIgnoringCollisions())
        {
            ScheduleCollisionCheck(entity, physicsRig, false);
        }
    }
    
    public static void ScheduleDespawnReset(CachedMarrowEntity entity)
    {
        foreach (var physicsRig in PhysicsRigCache.GetAll())
        {
            ScheduleCollisionCheck(entity, physicsRig, true);
        }
    }

    public static void ScheduleRigCollisions(CachedPhysicsRig cachedPhysicsRig, bool shouldCollide)
    {
        var stopwatch = Stopwatch.StartNew();
        
        foreach (var entity in MarrowEntityCache.GetAwake())
        {
            ScheduleCollisionCheck(entity, cachedPhysicsRig, shouldCollide);
        }
        
        stopwatch.Stop();
        InternalLogger.Debug($"Scheduled collision checks for {cachedPhysicsRig.PhysicsRig.name} with {MarrowEntityCache.GetAwake().Count()} entities in {stopwatch.ElapsedMilliseconds}ms.");
    }

    public static void OnUpdate()
    {
        if (Tasks.Count == 0)
            return;
        
        var stopwatch = Stopwatch.StartNew();
        var tasksProcessed = 0;
        while (tasksProcessed < TasksPerFrame)
        {
            if (stopwatch.ElapsedMilliseconds > 1) // 100ms
                break;
            
            if (!Tasks.TryDequeue(out var task))
                break;
            
            var entity = task.Entity;
            var physicsRig = task.PhysicsRig;
            var shouldCollide = task.ShouldCollide;
            
            entity.SetColliding(physicsRig, shouldCollide);
            tasksProcessed += entity.Colliders.Length;
        }
        
        stopwatch.Stop();
#if DEBUG
        InternalLogger.Debug($"Processed {tasksProcessed} collision tasks in {stopwatch.ElapsedMilliseconds}ms. Remaining tasks: {Tasks.Count}");
#endif
    }
}