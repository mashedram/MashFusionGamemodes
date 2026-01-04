using Clockhunt.Entities.Tags;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Entities.Tagging;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Clockhunt.Joke;

public static class FrogJumpscare
{
    private const int JumpScareChance = 1000; // 1 in X chance
    private static double _lastJumpScare;

    public static void PlayJumpscare(NetworkEntity entity)
    {
        _lastJumpScare = Time.timeSinceLevelLoadAsDouble;
        if (Random.RandomRange(0, JumpScareChance) != 0) return;

        var target = Camera.main?.transform.position;
        if (target == null) return;

        var marrow = entity.GetExtender<IMarrowEntityExtender>();
        if (marrow == null) return;
        var from = marrow.MarrowEntity.transform.position;

        var spawnable = LocalAssetSpawner.CreateSpawnable("Mash.ClockhuntAssets.Spawnable.FrogJump");
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, from, Quaternion.identity, poolee =>
        {
            if (poolee.gameObject.TryGetComponent<Rigidbody>(out var rb))
            {
                var offset = target.Value - from;
                rb.velocity = offset * 0.8f;
            }

            PooleeHelper.DespawnDelayed(poolee, 1f);
        });
    }

    public static void Update()
    {
        var currentTime = Time.timeSinceLevelLoadAsDouble;
        if (currentTime - _lastJumpScare < 10f) return;

        var cameraPos = Camera.main?.transform.position;
        if (cameraPos == null) return;

        foreach (var entry in ClockMarker.Query.Where(entry => entry.Instance.IsReady))
        {
            var marrowEntity = entry.Instance.MarrowEntity!;
            var distance = Vector3.Distance(marrowEntity.transform.position, (Vector3)cameraPos);
            if (distance > 8f) continue;

            PlayJumpscare(entry.Instance.NetworkEntity!);
            break;
        }
    }
}