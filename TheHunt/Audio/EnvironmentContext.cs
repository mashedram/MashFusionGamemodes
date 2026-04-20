using LabFusion.Entities;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Gamemode;
using TheHunt.Teams;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheHunt.Audio;

public class EnvironmentContext
{
    private const float ChaseDuration = 5f;
    private static readonly LayerMask PlayerLayerMask = Physics.DefaultRaycastLayers & ~(1 << 8);

    // Pow for sqrt distance check, as it's called every frame and we want to avoid the costly sqrt operation
    private static float TensionDistance = MathF.Pow(40f, 2);
    // After tension ends, we wait a bit before we exit the state
    private static float TensionDurationExtension => Random.RandomRange(0f, 8f);
    // Random chance for tension to just start even if the nightmare isn't chasing, to add some unpredictability
    private static float TensionSpookDelay => Random.RandomRange(90f, 160f);  

    private static float _chaseTimer;

    private static bool _inTension;
    private static float _tensionTimer;
    private static float _spookTimer = TensionSpookDelay;

    public bool IsChasing { get; private set; }
    public bool IsTension { get; private set; }
    
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
        _chaseTimer = 0f;
        _tensionTimer = 0f;
        _spookTimer = TensionSpookDelay;
    }

    private static bool ShouldBeChasing(TheHuntContext context, float delta)
    {
        var localPosition = context.LocalPlayer.RigRefs.Head.position;
        var shouldBeChasing = NetworkPlayer.Players.Any(player => player.PlayerID.IsTeam<NightmareTeam>() &&
                                                                  IsNightmareChasing(player, localPosition));
        _chaseTimer = shouldBeChasing ? ChaseDuration : Mathf.Max(0, _chaseTimer - delta);
        return _chaseTimer > 0.5f;
    }

    private static bool ShouldBeTension(float delta)
    {
        var shouldBeTension = !IsLocalNightmare &&
                              (NetworkPlayer.Players.Any(player => player.PlayerID.IsTeam<NightmareTeam>() &&
                                                                   player.DistanceSqr <= TensionDistance) ||
                               _spookTimer <= 1f
                              );
        if (shouldBeTension && !_inTension)
        {
            _inTension = true;
            _tensionTimer = TensionDurationExtension;
        }
        else
        {
            _tensionTimer = Mathf.Max(0, _tensionTimer - delta);
        }
        var isTension = _tensionTimer > 0.5f;
        // Quit tension
        if (isTension) 
            return isTension;
        
        _inTension = false;
        _spookTimer = TensionSpookDelay;

        return isTension;
    }
    
    public static EnvironmentContext GetContext(TheHuntContext context)
    {
        var delta = Time.deltaTime;
        
        if (IsLocalNightmare)
            return new EnvironmentContext
            {
                IsChasing = false,
                IsTension = false
            };

        // Chasing
        var isChasing = ShouldBeChasing(context, delta);
        var isTension = ShouldBeTension(delta);

        return new EnvironmentContext
        {
            IsChasing = isChasing,
            IsTension = isTension
        };
    }
}