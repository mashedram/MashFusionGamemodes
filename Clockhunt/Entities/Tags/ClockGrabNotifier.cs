using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Components;

namespace Clockhunt.Entities.Tags;

public class ClockGrabNotifier : IEntityGrabCallback, IEntityDropCallback
{
    public static readonly HashSet<PlayerID> Holders = new();

    public double DropCooldown => 0.05f;
    public double GrabCooldown => 0.05f;

    public void OnGrab(GrabData grab)
    {
        Holders.Add(grab.NetworkPlayer.PlayerID);
    }

    public void OnDrop(GrabData grab)
    {
        Holders.Remove(grab.NetworkPlayer.PlayerID);
    }
}