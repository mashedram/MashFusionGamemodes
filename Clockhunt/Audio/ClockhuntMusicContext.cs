using Clockhunt.Game;
using Clockhunt.Nightmare;
using Clockhunt.Phase;
using MashGamemodeLibrary.Phase;
using UnityEngine;

namespace Clockhunt.Audio;

public class ClockhuntMusicContext
{
    private const float ChaseDuration = 5f;
    private static readonly LayerMask PlayerLayerMask = Physics.DefaultRaycastLayers & ~(1 << 8);

    private static float _chaseTimer;
    private ITimedPhase? _phase = null!;

    public float PhaseProgress => _phase != null ? Mathf.Clamp01(_phase.ElapsedTime / _phase.Duration) : 1f;
    public bool IsChasing { get; private set; }
    public static bool IsLocalNightmare => WinStateManager.LocalGameTeam == GameTeam.Nightmares;

    public bool IsPhase<T>() where T : GamePhase
    {
        return _phase is T;
    }

    private static bool IsNightmareChasing(NightmareInstance nightmare, Vector3 localPosition)
    {
        if (!nightmare.Owner.HasRig)
            return false;

        var otherPosition = nightmare.Owner.RigRefs.Head.position;
        var line = otherPosition - localPosition;
        var distance = line.magnitude;

        var direction = line.normalized;
        var lineOfSight = !Physics.Raycast(localPosition, direction, distance, PlayerLayerMask);

        return nightmare.CanStartChaseMusic(nightmare.Owner, distance, lineOfSight);
    }

    public static void Reset()
    {
        _chaseTimer = 0f;
    }

    public static ClockhuntMusicContext GetContext(ClockhuntContext context)
    {
        var delta = Time.deltaTime;

        var localPosition = context.LocalPlayer.RigRefs.Head.position;
        var shouldBeChasing = WinStateManager.LocalGameTeam != GameTeam.Nightmares &&
                              NightmareManager.Nightmares.Any(nightmare =>
                                  IsNightmareChasing(nightmare, localPosition));

        _chaseTimer = shouldBeChasing ? ChaseDuration : Mathf.Max(0, _chaseTimer - delta);
        var isChasing = _chaseTimer > 0.5f;

        return new ClockhuntMusicContext
        {
            _phase = GamePhaseManager.ActivePhase as ITimedPhase,
            IsChasing = isChasing
        };
    }
}