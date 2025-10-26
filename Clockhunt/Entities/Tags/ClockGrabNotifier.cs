using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Components;

namespace Clockhunt.Entities.Tags;

public class ClockGrabNotifier : IEntityGrabCallback, IEntityDropCallback
{
    public static readonly HashSet<PlayerID> Holders = new();

    public double DropCooldown => 0.05f;

    public void OnDrop(GrabData grab)
    {
        if (grab.NetworkPlayer == null)
            return;
        
        Holders.Remove(grab.NetworkPlayer.PlayerID);
    }

    public double GrabCooldown => 0.05f;

    public void OnGrab(GrabData grab)
    {
        if (grab.NetworkPlayer == null)
            return;
        
        Holders.Add(grab.NetworkPlayer.PlayerID);
    }
}