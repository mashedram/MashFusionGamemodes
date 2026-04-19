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

        var otherPosition = nightmare.RigRefs.Head.position;
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

    public static EnvironmentContext GetContext(TheHuntContext context)
    {
        var delta = Time.deltaTime;

        // Chasing
        var localPosition = context.LocalPlayer.RigRefs.Head.position;
        var shouldBeChasing = !IsLocalNightmare &&
                              NetworkPlayer.Players.Any(player => player.PlayerID.IsTeam<NightmareTeam>() &&
                                                                  IsNightmareChasing(player, localPosition));
        _chaseTimer = shouldBeChasing ? ChaseDuration : Mathf.Max(0, _chaseTimer - delta);
        var isChasing = _chaseTimer > 0.5f;

        // Tension
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
        if (!isTension)
        {
            _inTension = false;
            _spookTimer = TensionSpookDelay;
        }

        return new EnvironmentContext
        {
            IsChasing = isChasing,
            IsTension = isTension
        };
    }
}