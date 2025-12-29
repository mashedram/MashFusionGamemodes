using LabFusion.Utilities;
using MashGamemodeLibrary.Data.Random;
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
            13,
            NavMeshCollectGeometry.PhysicsColliders,
            0,
            new Il2CppSystem.Collections.Generic.List<NavMeshBuildMarkup>(),
            sources
        );

        // Big agent radius to avoid spawning in a wall
        var settings = new NavMeshBuildSettings
        {
            agentHeight = 4f,
            agentRadius = 4f,
            agentSlope = 0f,
            agentClimb = 0f
        };
        
        _navMeshData = NavMeshBuilder.BuildNavMeshData(
            settings,
            sources,
            bounds,
            center,
            Quaternion.identity
        );
    }

    public static void SetRandomSpawn(int tries, params AvoidSpawningNear[] avoid)
    {
        if (_navMeshData == null)
        {
            InternalLogger.Debug("You forgot to collect spawnpoints before setting them");
            return;
        }
        
        // Start at 1 
        Vector3? target = null;
        for (var i = 1; i <= tries; i++)
        {
            if (!NavMesh.SamplePosition(_center + UnityEngine.Random.insideUnitSphere * _size, out NavMeshHit hit, _size, NavMesh.AllAreas))
                continue;

            target = hit.position;
            if (avoid.Any(a => (a.Position - target.Value).sqrMagnitude < a.HalfRadiusSquare))
                continue;
            
            break;
        }

        if (target == null)
        {
            MelonLogger.Error("Failed to find any spawnpoint! this is a bug.");
            return;
        }

        SetSpawn(target.Value);
    }
}