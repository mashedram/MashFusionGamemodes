using BoneStrike.Teams;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppTMPro;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Pool;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Phase;
using MashGamemodeLibrary.Player.Team;
using UnityEngine;

namespace BoneStrike.Phase;

public record SpawnTeamWallPacket : INetSerializable
{
    public Vector3 Position;
    public float Yaw;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Yaw);
    }
}

internal record struct PlayerWallSide(PlayerID PlayerID, bool IsInFront);

public class TeamAssignmentPhase : GamePhase
{
    private static readonly RemoteEvent<SpawnTeamWallPacket> SpawnTeamWallEvent = new(SpawnTeamWallHandler, CommonNetworkRoutes.HostToAll);

    private static readonly string TeamWallBarcode = "Mash.BoneStrike.Spawnable.TeamWALL";
    private static Poolee? _poolee;
    private static TextMeshPro[] _timerTexts = new TextMeshPro[2];

    private static Vector3 _wallPosition;
    private static Quaternion _wallRotation;
    private static bool _areTeamsFair;

    public override string Name => "Team Assignment Phase";

    public override float Duration => 15f;

    private bool IsPositionInFrontOfWall(Vector3 position)
    {
        var toPlayer = position - _wallPosition;
        var forward = _wallRotation * Vector3.forward;

        return Vector3.Dot(toPlayer, forward) > 0f;
    }

    private IEnumerable<PlayerWallSide> GetPlayerWallSides()
    {
        return from player in NetworkPlayer.Players
            where player.HasRig
            select new PlayerWallSide(player.PlayerID, IsPositionInFrontOfWall(player.RigRefs.Head.position));
    }

    public override PhaseIdentifier GetNextPhase()
    {
        if (!HasReachedDuration())
            return PhaseIdentifier.Empty();

        return PhaseIdentifier.Of<PlantPhase>();
    }

    private bool AreTeamsFair()
    {
        var wallSides = GetPlayerWallSides().ToList();
        var team1Count = wallSides.Count(p => p.IsInFront);
        var team2Count = wallSides.Count - team1Count;

        if (team1Count == 0 || team2Count == 0)
            return false;

        if (BoneStrike.Config.AllowUnbalancedTeams)
            return true;

        var imbalance = Mathf.Abs(team1Count - team2Count);
        if (imbalance > 2)
            return false;

        return true;
    }

    public override bool CanTimerTick()
    {
        _areTeamsFair = AreTeamsFair();
        return _areTeamsFair;
    }

    protected override void OnPhaseEnter()
    {
        Executor.RunIfHost(() =>
        {
            var wallTransform = LocalPlayer.GetNetworkPlayer()?.RigRefs.Head;
            if (wallTransform == null)
                return;

            var yaw = wallTransform.rotation.eulerAngles.y;

            SpawnTeamWallEvent.Call(new SpawnTeamWallPacket
            {
                Position = wallTransform.position,
                Yaw = yaw
            });
        });
    }

    protected override void OnPhaseExit()
    {
        Executor.RunIfHost(() =>
        {
            var sets = new List<PlayerID>[]
            {
                new(),
                new()
            };
            foreach (var player in NetworkPlayer.Players)
            {
                if (!player.HasRig)
                    continue;

                var teamIndex = IsPositionInFrontOfWall(player.RigRefs.Head.position) ? 0 : 1;
                sets[teamIndex].Add(player.PlayerID);
            }

            BoneStrike.Context.PersistentTeams.OverwritePlayerSets(sets);
        });

        if (_poolee == null)
            return;

        _poolee.Despawn();
        _poolee = null;
    }

    protected override void OnUpdate()
    {
        if (_poolee == null)
            return;

        if (!_areTeamsFair)
        {
            foreach (var text in _timerTexts)
            {
                text.color = Color.red;
                text.text = "X";
            }
            return;
        }

        var time = Duration - ElapsedTime;
        var minutes = Math.Max(Mathf.FloorToInt(time / 60f), 0);
        var seconds = Math.Max(Mathf.FloorToInt(time % 60f), 0);
        var textContent = $"{minutes:D1}:{seconds:D2}";
        foreach (var text in _timerTexts)
        {
            text.color = Color.white;
            text.text = textContent;
        }
    }

    // Event Handler

    private static void SpawnTeamWall(Vector3 position, Quaternion rotation)
    {
        _wallPosition = position;
        _wallRotation = rotation;

        if (_poolee != null)
        {
            var transform = _poolee.transform;
            transform.position = position;
            transform.rotation = rotation;
            return;
        }

        var spawnable = LocalAssetSpawner.CreateSpawnable(TeamWallBarcode);
        LocalAssetSpawner.Register(spawnable);
        LocalAssetSpawner.Spawn(spawnable, position, rotation, poolee =>
        {
            _poolee = poolee;
            _timerTexts = poolee.GetComponentsInChildren<TextMeshPro>();
        });
    }

    private static void SpawnTeamWallHandler(SpawnTeamWallPacket packet)
    {
        var rotation = Quaternion.Euler(0f, packet.Yaw, 0f);
        SpawnTeamWall(packet.Position, rotation);
    }
}