using LabFusion.Entities;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Gamemode;
using TheHunt.Phase;
using TheHunt.Teams;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheHunt.Audio;

public class EnvironmentContext
{
    private const float MaxChaseTime = 15f;
    private const float ChaseThreshold = 6f;
    private const float UnChaseThreshold = 2f;
    private static readonly LayerMask PlayerLayerMask = Physics.DefaultRaycastLayers & ~(1 << 8);

    private static bool _isChasing;
    private static float _chaseTimer;

    public bool IsChasing { get; private set; }
    
    public static bool IsLocalNightmare => LogicTeamManager.IsLocalTeam<NightmareTeam>();

    public bool IsPhase<T>() where T : GamePhase
    {
        return GamePhaseManager.IsPhase<T>();
    }

    private static bool IsNightmareChasing(NetworkPlayer nightmare, Vector3 localPosition)
    {
        if (!nightmare.HasRig)
            return false;

        var nightmareHead = nightmare.RigRefs.Head;
        if (nightmareHead == null)
            return false;
        
        var nightmarePosition = nightmareHead.position;
        
        // Check if we can actually see the entity, head angle wise
        // We only do this check if the chase just started, to avoid the nightmare "losing" the player if they look away for a second
        if (_chaseTimer < 0f)
        {
            var toEntity = nightmarePosition - localPosition;
            var forward = nightmareHead.forward;
            var angle = Vector3.Angle(forward, toEntity);
            if (angle > 100f)
                return false;
        }

        var otherPosition = nightmarePosition;
        var line = otherPosition - localPosition;
        var distance = line.magnitude;

        var direction = line.normalized;
        var lineOfSight = !Physics.Raycast(localPosition, direction, distance, PlayerLayerMask);

        return lineOfSight;
    }

    public static void Reset()
    {
        _isChasing = false;
        _chaseTimer = 0f;
    }

    private static bool ShouldBeChasing(TheHuntContext context, float delta)
    {
        if (GamePhaseManager.ActivePhase is HidePhase)
            return false;
        
        var localPosition = context.LocalPlayer.RigRefs.Head.position;
        var shouldBeChasing = NetworkPlayer.Players.Any(player => player.PlayerID.IsTeam<NightmareTeam>() &&
                                                                  IsNightmareChasing(player, localPosition));
        
        // Increase or decrease the timer based on whether we should be chasing
        _chaseTimer = shouldBeChasing ? MathF.Min(_chaseTimer + delta, MaxChaseTime) : MathF.Max(_chaseTimer - delta, 0f);
        
        if (_isChasing && _chaseTimer <= UnChaseThreshold)
        {
            _isChasing = false;
        }
        else if (!_isChasing && _chaseTimer >= ChaseThreshold)
        {
            _isChasing = true;
        }
        
        return _isChasing;
    }
    
    public static EnvironmentContext GetContext(TheHuntContext context)
    {
        var delta = Time.deltaTime;
        
        if (IsLocalNightmare)
            return new EnvironmentContext
            {
                IsChasing = false,
            };

        // Chasing
        var isChasing = ShouldBeChasing(context, delta);

        return new EnvironmentContext
        {
            IsChasing = isChasing
        };
    }
}