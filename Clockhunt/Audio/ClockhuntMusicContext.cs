using Clockhunt.Game;
using Clockhunt.Nightmare;
using MashGamemodeLibrary.Phase;
using UnityEngine;

namespace Clockhunt.Audio;

public class ClockhuntMusicContext
{
    private const float ChaseDuration = 15f;
    private static LayerMask PlayerLayerMask = Physics.DefaultRaycastLayers & ~(1 << 8);
    
    private static float _chaseTimer = 0f;
    private GamePhase Phase;
    
    public float PhaseProgress => Mathf.Clamp01(Phase.ElapsedTime / Phase.Duration);
    public bool IsChasing { get; private set; }
    public bool IsLocalNightmare => WinStateManager.LocalGameTeam == GameTeam.Nightmares;
    
    public bool IsPhase<T>() where T : GamePhase
    {
        return Phase is T;
    }

    private static bool IsNightmareChasing(NightmareInstance nightmare, Vector3 localPosition)
    {
        var otherPosition = nightmare.Owner.RigRefs.Head.position;
        var line = otherPosition - localPosition;
        var distance = line.magnitude;

        var direction = line.normalized;
        var lineOfSight = !Physics.Raycast(localPosition, direction, distance, PlayerLayerMask);

        return nightmare.CanStartChaseMusic(nightmare.Owner, distance, lineOfSight);
    }
    
    public static ClockhuntMusicContext GetContext(ClockhuntContext context)
    {
        var delta = Time.deltaTime;

        var localPosition = context.LocalPlayer.RigRefs.Head.position;
        var shouldBeChasing = WinStateManager.LocalGameTeam != GameTeam.Nightmares && 
                        NightmareManager.Nightmares.Any(nightmare => IsNightmareChasing(nightmare, localPosition));

        _chaseTimer = shouldBeChasing ? ChaseDuration : Mathf.Max(0, _chaseTimer - delta);
        var isChasing = _chaseTimer > 0f;
        
        return new ClockhuntMusicContext
        {
            Phase = context.PhaseManager.GetActivePhase(),
            IsChasing = isChasing
        };
    }
}