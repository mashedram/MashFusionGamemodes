using UnityEngine;

namespace MashGamemodeLibrary.Audio.Modifiers;

internal struct RayPassResult
{
    public bool HitWall;
    public Vector3 WallEndPosition;
    public float WallThickness;
}

public class MuffleAudioModifier : IAudioModifier
{
    private const int BaseCutoffFrequency = 5000;
    private const int MaxRayPasses = 2;

    private static readonly LayerMask
        RaycastLayerMask = Physics.DefaultRaycastLayers & ~(1 << 8); // Ignore player layer

    private AudioLowPassFilter _filter = null!;

    public void OnStart(ref AudioSource source)
    {
        _filter = source.transform.GetComponent<AudioLowPassFilter>();
        if (_filter == null)
            _filter = source.transform.gameObject.AddComponent<AudioLowPassFilter>();

        _filter.cutoffFrequency = 5000f;
        _filter.lowpassResonanceQ = 1f;
    }

    public void Update(ref AudioSource source, float delta)
    {
        var playerPosition = Camera.main?.transform.position;
        if (!playerPosition.HasValue)
            return;

        var soundPosition = source.transform.position;
        var toPlayerOffset = playerPosition.Value - soundPosition;

        if (toPlayerOffset.magnitude > source.maxDistance) return;

        var toPlayerDirection = toPlayerOffset.normalized;

        var rayResult = PerformRayPass(playerPosition.Value, soundPosition + toPlayerDirection, MaxRayPasses);
        if (!rayResult.HitWall)
        {
            _filter.enabled = false;
            return;
        }

        _filter.enabled = true;
        var thickness = rayResult.WallThickness;
        // MelonLogger.Msg($"Wall thickness: {thickness}");
        var cutoff = BaseCutoffFrequency / Mathf.Pow(thickness * 0.1f, 2);
        cutoff = Mathf.Clamp(cutoff, 1500f, BaseCutoffFrequency);

        _filter.cutoffFrequency = cutoff;
    }

    private RayPassResult PerformRayPass(Vector3 origin, Vector3 target, int passIndex)
    {
        var offset = target - origin;
        var distance = offset.magnitude;
        var direction = offset / distance;

        var ray = new Ray(origin, direction);
        // No walls hit
        if (!Physics.Raycast(ray, out var hitInfo, distance, RaycastLayerMask))
            return new RayPassResult
            {
                HitWall = false,
                WallEndPosition = target,
                WallThickness = 0f
            };

        // Send a ray back to check where the wall ends
        var backRay = new Ray(target, -direction);
        if (!hitInfo.collider.Raycast(backRay, out var backHit, distance))
            // We are in the wall
            return new RayPassResult
            {
                HitWall = true,
                WallEndPosition = target,
                WallThickness = distance
            };

        var wallThickness = (backHit.point - hitInfo.point).magnitude;
        if (passIndex <= 0)
            return new RayPassResult
            {
                HitWall = true,
                WallEndPosition = backHit.point,
                WallThickness = wallThickness
            };

        // We still have passes to go

        var nextPass = PerformRayPass(backHit.point + direction * 0.01f, target, passIndex - 1);

        // Accumulate wall thickness
        return new RayPassResult
        {
            HitWall = true,
            WallEndPosition = nextPass.WallEndPosition,
            WallThickness = wallThickness + nextPass.WallThickness
        };
    }
}