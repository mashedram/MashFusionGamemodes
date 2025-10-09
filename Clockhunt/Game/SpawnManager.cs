using System.Collections.Immutable;
using Clockhunt.Util;
using LabFusion.Entities;
using LabFusion.Extensions;
using UnityEngine;

namespace Clockhunt.Game;

class LocalSpawnCollector
{
    private const double NodeCollectionInterval = 1f;
    private const int MaxNodes = 20;
    private const float PlayerHeight = 2f;
    
    private static readonly LayerMask GroundLayerMask = Physics.DefaultRaycastLayers & ~(1 << 8); // Ignore player layer
    
    private int _nodeIndex;
    private Vector3[] _nodes = new Vector3[MaxNodes];
    private double _lastNodeCollectionTime;
    
    public event Action<Vector3>? OnCollect;
    
    private NetworkPlayer GetLocalPlayer()
    {
        return Clockhunt.Context.LocalPlayer;
    }
    
    private Vector3? GetFloorPosition()
    {
        var player = GetLocalPlayer();
        var rigManager = player.RigRefs?.RigManager;
        if (rigManager == null)
            return null;
        
        if (rigManager.physicsRig.footSupported < 0.1f)
            return null;
        
        var origin = rigManager.physicsRig._football.center;
        var distance = rigManager.physicsRig._footballRadius * 2f;
        
        var ray = new Ray(origin, Vector3.down);
        if (!Physics.Raycast(ray, out var hitInfo, distance, GroundLayerMask))
            return null;

        var floor = hitInfo;
        var center = floor.point + Vector3.up * PlayerHeight;
        var halfExtents = new Vector3(0.5f, PlayerHeight * 0.45f, 0.5f);
        if (Physics.CheckBox(center, halfExtents, Quaternion.identity, GroundLayerMask))
            return null;
        
        return hitInfo.point;
    }
    
    private void CollectPoints()
    {
        var size = _nodeIndex;
        _nodeIndex = 0;
        
        if (size <= 0)
            return;

        var index = size / 2;
        var position = _nodes[index];
        
        OnCollect?.Invoke(position);
    }

    public void Update(float delta)
    {
        var time = Time.timeSinceLevelLoadAsDouble;
        if (time - _lastNodeCollectionTime < NodeCollectionInterval)
            return;
        _lastNodeCollectionTime = time;

        var position = GetFloorPosition();
        if (position == null)
        {
            CollectPoints();
            return;
        }
        
        _nodes[_nodeIndex] = position.Value;
        
        if (_nodeIndex >= MaxNodes)
            CollectPoints();
        
        _nodeIndex += 1;
    }
}

public static class SpawnManager
{
    private const float MinDistanceBetweenSpawns = 5f;
    
    private static readonly LocalSpawnCollector Collector = new();
    private static readonly HashSet<Vector3> SpawnPoints = new();

    static SpawnManager()
    {
        Collector.OnCollect += OnCollect;
    }

    private static void OnCollect(Vector3 position)
    {
        var distance = SpawnPoints.Any()
            ? SpawnPoints.Min(x => Vector3.Distance(x, position))
            : float.MaxValue;
        
        // If it's far enough away from other spawn points, add it
        if (distance < MinDistanceBetweenSpawns)
            return;
        
        SpawnPoints.Add(position);
    }
    
    private static void RemoveUntilCount(int count)
    {
        var toRemove = SpawnPoints.Count - count;
        if (toRemove <= 0)
            return;

        var spawnPoints = SpawnPoints
            .GroupBy(position => SpawnPoints.Min(other => Vector3.Distance(position, other)))
            .OrderBy(group => group.Key);

        foreach (var point in spawnPoints.Take(toRemove))
        {
            point.ExceptLast().ForEach(item => SpawnPoints.Remove(item));
        }
    }

    public static void Update(float delta)
    {
        Collector.Update(delta);
    }
    
    
}