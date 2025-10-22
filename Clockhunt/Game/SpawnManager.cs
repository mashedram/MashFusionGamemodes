using Clockhunt.Util;
using LabFusion.Entities;
using LabFusion.Extensions;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using UnityEngine;
#if DEBUG
#endif

namespace Clockhunt.Game;

internal class TimedCaller
{
    public delegate void TimerCallback();

    private readonly TimerCallback _callback;

    private float _delay;
    private float _timer;

    public TimedCaller(float delay, TimerCallback callback)
    {
        _delay = delay;
        _callback = callback;
    }

    public void SetDelay(float delay)
    {
        _delay = delay;
    }

    public void Update(float delta)
    {
        _timer = Math.Max(_timer - delta, 0);
        if (_timer > 0.1f) return;

        _timer = _delay;

        _callback.Invoke();
    }
}

internal struct SpawnObjectInstance
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
    private const int MaxCollectedSpawns = 300;

    private static readonly SyncedSet<Vector3> CollectedSpawnPoints =
        new("CollectedSpawnPoints", new Vector3Encoder(), CommonNetworkRoutes.BiDirectional);

    private static readonly SyncedSet<Vector3> SyncedSpawnPoints = new("SpawnPoints", new Vector3Encoder(), CommonNetworkRoutes.AllToHost);

    private static readonly LinkedList<SpawnObjectInstance> SpawnObjects = new();

    private static readonly TimedCaller TimedSpawnCollector = new(5f, CollectSpawnPoint);

    private static void CollectSpawnPoint()
    {
        var player = Clockhunt.Context.LocalPlayer;
        var rigManager = player.RigRefs?.RigManager;

        if (rigManager == null)
            return;

        if (rigManager.remapHeptaRig._jumping)
            return;

        if (rigManager.physicsRig.footSupported < 0.1f)
            return;

        if (rigManager.remapHeptaRig._crouchTarget < -0.2f)
            return;

        var position = rigManager.physicsRig.m_pelvis.position;
        CollectedSpawnPoints.Add(position);
    }

    public static void SubmitSynced(int count)
    {
        var spawnPoints = CollectedSpawnPoints
            .GroupBy(position =>
                CollectedSpawnPoints.Where(p => p != position).Min(other => Vector3.Distance(position, other)))
            .OrderByDescending(group => group.Key)
            .Take(count);

        SyncedSpawnPoints.Clear();
        foreach (var group in spawnPoints)
        {
            var spawn = group.GetRandom();
            SyncedSpawnPoints.Add(spawn);
        }
    }

    public static void Update(float delta)
    {
        if (CollectedSpawnPoints.Count > MaxCollectedSpawns)
            return;

        TimedSpawnCollector.Update(delta);
    }

    public static void Reset()
    {
        // We want to collect all the points over 2 minutes of gametime
        const float sampleDurationSeconds = 120f;
        var playerCount = NetworkPlayer.Players.Count;
        var samplesPerPlayer = MaxCollectedSpawns / playerCount;
        var sampleInterval = Math.Max(sampleDurationSeconds / samplesPerPlayer, 1f);
        TimedSpawnCollector.SetDelay(sampleInterval);

        Executor.RunIfHost(() => { CollectedSpawnPoints.Clear(); });
    }

    public static Transform[] GetSpawnPoints()
    {
        // Populate the list
        for (var i = SpawnObjects.Count; i < SyncedSpawnPoints.Count; i++)
            SpawnObjects.AddLast(new SpawnObjectInstance());

        using var syncedSpawnPointsEnumerator = SyncedSpawnPoints.GetEnumerator();
        var list = new Transform[SyncedSpawnPoints.Count];
        foreach (var (index, spawnObjectInstance) in SpawnObjects.WithIndices())
        {
            var go = spawnObjectInstance.GetOrCreate();
            go.transform.position = syncedSpawnPointsEnumerator.Current;
            list[index] = go.transform;

            syncedSpawnPointsEnumerator.MoveNext();
        }

        return list;
    }

    public static IEnumerable<Vector3> GetEnumerator()
    {
        return SyncedSpawnPoints;
    }
}