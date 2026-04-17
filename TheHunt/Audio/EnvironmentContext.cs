using LabFusion.Entities;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;
using TheHunt.Gamemode;
using TheHunt.Teams;
using UnityEngine;

namespace TheHunt.Audio;

public class EnvironmentContext
{
    private const float ChaseDuration = 5f;
    private static readonly LayerMask PlayerLayerMask = Physics.DefaultRaycastLayers & ~(1 << 8);

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
    }

    public static EnvironmentContext GetContext(TheHuntContext context)
    {
        var delta = Time.deltaTime;

        var localPosition = context.LocalPlayer.RigRefs.Head.position;
        var shouldBeChasing = !IsLocalNightmare &&
                              NetworkPlayer.Players.Any(player => player.PlayerID.IsTeam<NightmareTeam>() &&
                                                                  IsNightmareChasing(player, localPosition));

        _chaseTimer = shouldBeChasing ? ChaseDuration : Mathf.Max(0, _chaseTimer - delta);
        var isChasing = _chaseTimer > 0.5f;

        return new EnvironmentContext
        {
            IsChasing = isChasing
        };
    }
}