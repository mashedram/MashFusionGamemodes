using System.Diagnostics;
using LabFusion.Utilities;
using MashGamemodeLibrary.Data.Random;
using MashGamemodeLibrary.Debug;
using MashGamemodeLibrary.Util;
using MelonLoader;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

namespace MashGamemodeLibrary.Player.Spawning;

public record struct AvoidSpawningNear
{
    public Vector3 Position;
    internal float HalfRadiusSquare { get; }
    public AvoidSpawningNear(Vector3 position, float size)
    {
        Position = position;
        HalfRadiusSquare = Mathf.Pow(size / 2f, 2);
    }
}

public static class DynamicSpawnCollector
{
    private const float SafeRadius = 4f;
    private const int StaticLayer = 13;
    private static GameObject? _spawnGameObject = null;
    
    private static Vector3 _center = Vector3.zero;
    private static float _size;
    private static NavMeshData? _navMeshData;

    private static void SetSpawn(Vector3 position)
    {
        if (_spawnGameObject == null)
        {
            _spawnGameObject = new GameObject();
        }

        _spawnGameObject.transform.position = position;
        FusionPlayer.SetSpawnPoints(_spawnGameObject.transform);
    }

    public static void CollectAt(Vector3 center, float size)
    {
        _center = center;
        _size = size / 2f;
        var bounds = new Bounds(center, Vector3.one * size);

        var sources = new Il2CppSystem.Collections.Generic.List<NavMeshBuildSource>();
        
        NavMeshBuilder.CollectSources(
            bounds,
            // Bonelab static layer
            StaticLayer,
            NavMeshCollectGeometry.PhysicsColliders,
            0,
            new Il2CppSystem.Collections.Generic.List<NavMeshBuildMarkup>(),
            sources
        );

        // Big agent radius to avoid spawning in a wall
        var settings = new NavMeshBuildSettings
        {
            agentHeight = SafeRadius,
            agentRadius = SafeRadius,
            agentSlope = 0f,
            agentClimb = 0f,
            m_LedgeDropHeight = 50f,
            m_MaxJumpAcrossDistance = SafeRadius + 4f
        };
        
        _navMeshData = NavMeshBuilder.BuildNavMeshData(
            settings,
            sources,
            bounds,
            center,
            Quaternion.identity
        );
    }

    private static Vector3 GetReachablePoint(Vector3 origin, Vector3 direction, float distance)
    {
        var upRay = new Ray(origin, direction);
        return Physics.Raycast(upRay, out var upperHit, distance, StaticLayer) 
            // Reverse the direction by a bit to step out of a wall or ceiling
            ? upperHit.point + direction * -Math.Min(1f, upperHit.distance)
            // Get the max traversable distance
            : upRay.GetPoint(distance);
    }

    private static bool CanReachAny(Vector3 source, params Vector3[] targets)
    {
        return targets.Any(t =>
        {
            var direction = t - source;
            var ray = new Ray(source, direction.normalized);
                
            // If there is something in the way, quit
            if (Physics.Raycast(ray, direction.magnitude, StaticLayer))
                return false;
                
            // DebugRenderer.RenderLine(source, t, Color.yellow);
            return true;
        });
    }
    
    public static Vector3? GetRandomPoint(int tries, Vector3 canReach, params AvoidSpawningNear[] avoid)
    {
        if (_navMeshData == null)
        {
            InternalLogger.Debug("You forgot to collect spawnpoints before setting them");
            return null;
        }
        
        // Sample some ray check locations

        const float rayDistance = 10f;
        var reachPoints = new[]
        {
            canReach,
            GetReachablePoint(canReach, Vector3.up, rayDistance),
            GetReachablePoint(canReach, Vector3.down, rayDistance),
        };
        
        // Check actual areas
        // DebugRenderer.Clear();
        for (var i = 0; i < tries; i++)
        {
            if (!NavMesh.SamplePosition(_center + UnityEngine.Random.insideUnitSphere * _size, out NavMeshHit hit, SafeRadius, NavMesh.AllAreas))
                continue;

            var target = hit.position;
            if (avoid.Any(a => (a.Position - target).sqrMagnitude < a.HalfRadiusSquare))
                continue;

            // DebugRenderer.RenderCube(target, Vector3.one, Color.blue);

            // We don't care if this succeeds or fails
            var tempPath = new NavMeshPath();
            NavMesh.CalculatePath(target, canReach, NavMesh.AllAreas, tempPath);
            
            // DebugRenderer.RenderCube(_center, Vector3.one, Color.green);
            // foreach (var tempPathCorner in tempPath.corners)
            // {
            //     DebugRenderer.RenderCube(tempPathCorner, Vector3.one, Color.yellow);
            // }

            // Check paths for validity
            // If the navmesh failed to find a path, just check for direct LOS
            var start = tempPath.corners.LastOrDefault(target);
            if (!CanReachAny(start, reachPoints))
                continue;
            
            // DebugRenderer.RenderCube(target, Vector3.one, Color.red);
                
            InternalLogger.Debug($"Found spawn point at: {target.x} {target.y} {target.z}");
            return target;
        }

        return null;
    }

    public static void SetRandomSpawn(int tries, Vector3 canReach, params AvoidSpawningNear[] avoid)
    {
        var target = GetRandomPoint(tries, canReach, avoid);

        if (target == null)
        {
            MelonLogger.Error("Failed to find any spawnpoint! this is a bug.");
            return;
        }

        SetSpawn(target.Value);
    }
}