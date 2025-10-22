using Clockhunt.Config;
using Clockhunt.Entities.Tags;
using Clockhunt.Game.Teams;
using Clockhunt.Nightmare;
using Clockhunt.Vision;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using LabFusion.UI.Popups;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Phase;
using UnityEngine;

namespace Clockhunt.Game;

internal class OnEscapePointActivatedPacket : INetSerializable
{
    public Vector3 EscapePoint;

    public int? GetSize()
    {
        return sizeof(float) * 3;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref EscapePoint);
    }
}

internal class OnEscapeRequestPacket : INetSerializable
{
    public byte PlayerID;

    public int? GetSize()
    {
        return 1;
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerID);
    }
}

public static class EscapeManager
{
    private static readonly List<Vector3> EscapePoints = new();

    public static Vector3 GetRandomEscapePoint()
    {
        return EscapePoints.GetRandom();
    }

    public static void CollectEscapePoints()
    {
        Executor.RunIfHost(() =>
        {
            EscapePoints.Clear();

            foreach (var networkEntity in EntityTagManager.GetAllWithTag<ClockMarker>())
            {
                var position = networkEntity.GetExtender<IMarrowEntityExtender>()?.MarrowEntity?.transform.position;
                if (position == null)
                    continue;

                EscapePoints.Add(position.Value);
            }
        });
    }
}