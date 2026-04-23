using LabFusion.Extensions;
using LabFusion.Utilities;
using MashGamemodeLibrary.Debug;
using MashGamemodeLibrary.Util;
using MelonLoader;
using UnityEngine;
using UnityEngine.AI;
using Color = UnityEngine.Color;

namespace MashGamemodeLibrary.Player.Spawning;

public record struct AvoidSpawningNear
{
    public Vector3 Position;
    internal float Radius { get; }
    internal float RadiusSquare { get; }
    public AvoidSpawningNear(Vector3 position, float radius)
    {
        Position = position;
        Radius = radius;
        RadiusSquare = Mathf.Pow(radius, 2);
    }
}

public static class DynamicSpawnCollector
{
    private const float SafeRadius = 4f;
    private const int StaticLayer = 13;
    private const int DefaultLayer = 0;
    private static GameObject? _spawnGameObject;

    private static Vector3 _center = Vector3.zero;
    private static float _radius;
    private static NavMeshData? _navMeshData;

    private static List<Vector3> _validSpawnPoints = new();

    private static void SetSpawn(Vector3 position)
    {
        if (_spawnGameObject == null)
        {
            _spawnGameObject = new GameObject();
        }

        _spawnGameObject.transform.position = position;
        FusionPlayer.SetSpawnPoints(_spawnGameObject.transform);
    }

    public static void CollectAt(Vector3 center, float radius)
    {
        _center = center;
        _radius = radius;
        var bounds = new Bounds(center, Vector3.one * (radius * 2f));

        var sources = new Il2CppSystem.Collections.Generic.List<NavMeshBuildSource>();

        NavMeshBuilder.CollectSources(
            bounds,
            // Bonelab static layer
            1 << StaticLayer | 1 << DefaultLayer,
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

        _validSpawnPoints.Clear();
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
            var distance = Vector3.Distance(source, t);
            if (distance < 1f)
            {
                DebugRenderer.RenderLine(source, t, Color.cyan);
                return true;
            }

            var direction = t - source;
            var ray = new Ray(source, direction.normalized);

            // If there is something in the way, quit
            if (Physics.Raycast(ray, direction.magnitude, StaticLayer))
                return false;

            DebugRenderer.RenderLine(source, t, Color.yellow);
            return true;
        });
    }

    private static bool CanWalkPath(IReadOnlyList<Vector3> nodes)
    {
        var playerHeightOffset = Vector3.up * 1f;

        for (var i = 0; i < nodes.Count - 1; i++)
        {
            var nextIndex = i + 1;
            var from = nodes[i] + playerHeightOffset;
            var to = nodes[nextIndex] + playerHeightOffset;

            var direction = to - from;
            var ray = new Ray(from, direction.normalized);
            if (Physics.Raycast(ray, direction.magnitude, StaticLayer))
            {
                DebugRenderer.RenderLine(from, to, Color.red);
                return false;
            }

            DebugRenderer.RenderLine(from, to, Color.green);
        }

        return true;
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
            GetReachablePoint(canReach, Vector3.down, rayDistance)
        };

        var halfRadius = _radius / 2f;

        // Check actual areas
        DebugRenderer.Clear();
        var fallbackPositions = new SortedList<int, Vector3>();
        for (var i = 0; i < tries; i++)
        {
            var center = canReach + UnityEngine.Random.insideUnitSphere * halfRadius;

            if (!NavMesh.SamplePosition(center, out var hit, _radius, NavMesh.AllAreas))
                continue;

            var target = hit.position;

            // Check if we can reach the point
            DebugRenderer.RenderCube(target, Vector3.one, Color.blue);

            // We don't care if this succeeds or fails
            var tempPath = new NavMeshPath();
            NavMesh.CalculatePath(target, canReach, NavMesh.AllAreas, tempPath);

            DebugRenderer.RenderCube(_center, Vector3.one, Color.green);
            foreach (var tempPathCorner in tempPath.corners)
            {
                DebugRenderer.RenderCube(tempPathCorner, Vector3.one, Color.yellow);
            }

            if (tempPath.status == NavMeshPathStatus.PathPartial)
                target = tempPath.corners.Last();

            // Check paths for validity
            // If the navmesh failed to find a path, just check for direct LOS
            var start = tempPath.corners.LastOrDefault(target);
            if (!CanReachAny(start, reachPoints))
                continue;

            if (!CanWalkPath(tempPath.corners))
                continue;

            // Check if this is only valid as a fallback

            var avoidsInRange = avoid.Count(a => (a.Position - target).sqrMagnitude < a.RadiusSquare);
            if (avoidsInRange > 0)
            {
                fallbackPositions.Add(avoid.Length - avoidsInRange, target);
                continue;
            }

            DebugRenderer.RenderCube(target, Vector3.one, Color.red);

            InternalLogger.Debug($"Found spawn point at: {target.x} {target.y} {target.z}");
            _validSpawnPoints.Add(target);
            return target;
        }

        InternalLogger.Debug("Failed to find valid spawn, falling back to fallbacks");
        foreach (var (_, target) in fallbackPositions)
        {
            var tempPath = new NavMeshPath();
            NavMesh.CalculatePath(target, canReach, NavMesh.AllAreas, tempPath);

            var start = tempPath.corners.LastOrDefault(target);
            if (!CanReachAny(start, reachPoints))
                continue;

            if (!CanWalkPath(tempPath.corners))
                continue;

            InternalLogger.Debug($"Found fallback spawn point at: {target.x} {target.y} {target.z}");
            return target;
        }

        return _validSpawnPoints.Count > 0
            ? _validSpawnPoints.GetRandom()
            : null;
    }

    public static void SetRandomSpawn(int tries, Vector3 fallback, Vector3 canReach, params AvoidSpawningNear[] avoid)
    {
        var target = GetRandomPoint(tries, canReach, avoid);

        if (target == null)
        {
            MelonLogger.Error("Failed to find any spawnpoint! this is a bug.");
            SetSpawn(fallback);
            return;
        }

        SetSpawn(target.Value);
    }
}