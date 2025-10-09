using System.Collections.Immutable;
using Clockhunt.Util;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.networking.Variable.Impl;
using UnityEngine;

#if DEBUG
using MashGamemodeLibrary.Debug;
#endif

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
        
        var origin = rigManager.physicsRig._football.gameObject.transform.position;
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

    public void Reset()
    {
        _nodeIndex = 0;
    }
}

class SpawnCollectedPacket : INetSerializable
{
    public Vector3 Position;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Position);
    }
}

struct SpawnObjectInstance
{
    private GameObject? _gameObject;

    public GameObject GetOrCreate()
    {
        if (!_gameObject)
            _gameObject = new GameObject("PlayerSpawn");
        
        return _gameObject!;
    }
}

public static class SpawnManager
{
    private const float MinDistanceBetweenSpawns = 5f;

    private static readonly RemoteEvent<SpawnCollectedPacket> SpawnCollectedEvent =
        new("SpawnCollectedEvent", packet => OnCollect(packet.Position), false);
    private static readonly LocalSpawnCollector Collector = new();
    private static readonly HashSet<Vector3> SpawnPoints = new();
    private static readonly Vector3SyncedSet SyncedSpawnPoints = new("SpawnPoints");

    private static readonly LinkedList<SpawnObjectInstance> SpawnObjects = new();

    static SpawnManager()
    {
        Collector.OnCollect += OnCollect;
    }

    private static void OnCollect(Vector3 position)
    {
        if (!NetworkInfo.IsHost)
        {
            SpawnCollectedEvent.CallFor(PlayerIDManager.GetHostID(), new SpawnCollectedPacket
            {
                Position = position
            });
            return;
        }
        
        var distance = SpawnPoints.Any()
            ? SpawnPoints.Min(x => Vector3.Distance(x, position))
            : float.MaxValue;
        
        // If it's far enough away from other spawn points, add it
        if (distance < MinDistanceBetweenSpawns)
            return;
        
        SpawnPoints.Add(position);
        
        #if DEBUG
        DebugRenderer.RenderCube(position, Vector3.one);
        #endif
    }
    
    public static void SubmitSynced(int count)
    {
        var toRemove = Math.Max(SpawnPoints.Count - count, 0);

        var spawnPoints = SpawnPoints
            .GroupBy(position => SpawnPoints.Min(other => Vector3.Distance(position, other)))
            .OrderBy(group => group.Key)
            .Skip(toRemove);

        SyncedSpawnPoints.Clear();
        foreach (var group in spawnPoints)
        {
            foreach (var spawn in group)
            {
                SyncedSpawnPoints.Add(spawn);
            }
        }
    }

    public static void Update(float delta)
    {
        Collector.Update(delta);
    }

    public static void Reset()
    {
       Collector.Reset();
       
       Executor.RunIfHost(() =>
       {
           SpawnPoints.Clear();
       });
    }

    public static Transform[] GetSpawnPoints()
    {
        // Populate the list
        for (var i = SpawnObjects.Count; i < SpawnPoints.Count; i++)
        {
            SpawnObjects.AddLast(new SpawnObjectInstance());
        }
        
        var list = new Transform[SpawnPoints.Count];
        foreach (var (index, spawnObjectInstance) in SpawnObjects.WithIndices())
        {
            var go = spawnObjectInstance.GetOrCreate();
            list[index] = go.transform;
        }

        return list;
    }

    public static IEnumerable<Vector3> GetEnumerator()
    {
        return SpawnPoints;
    }
}