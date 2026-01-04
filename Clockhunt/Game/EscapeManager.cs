using Clockhunt.Config;
using Clockhunt.Entities.Tags;
using Clockhunt.Game.Teams;
using Clockhunt.Nightmare;
using Clockhunt.Vision;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Integration;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Gamemodes;
using LabFusion.UI.Popups;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Phase;
using UnityEngine;

namespace Clockhunt.Game;

public static class EscapeManager
{
    public static Vector3? EscapePoint { get; private set; } = null;

    private static IEnumerable<Vector3> GetEscapePoints()
    {
        var markers = ClockMarker.Query
            .Where(entry => entry.Instance.IsReady)
            .Select(entry => entry.Instance.NetworkEntity!).
            ToList();

        if (markers is { Count: > 0 })
            return markers.Select(networkEntity => networkEntity.GetExtender<IMarrowEntityExtender>()?.MarrowEntity?.transform.position).OfType<Vector3>();

        var spawns = FusionPlayer.SpawnPoints;
        if (spawns is { Count: > 0 })
            return spawns.Select(t => t.position);

        var gameSpawns = GamemodeMarker.FilterMarkers();
        if (gameSpawns is { Count: > 0 })
            return gameSpawns.Select(spawn => spawn.transform.position);

        return new[]
        {
            Clockhunt.Context.LocalPlayer.RigRefs.RigManager.transform.position
        };
    }

    public static void CollectEscapePoints()
    {
        Executor.RunIfHost(() =>
        {
            EscapePoint = GetEscapePoints().GetRandom();
        });
    }
}