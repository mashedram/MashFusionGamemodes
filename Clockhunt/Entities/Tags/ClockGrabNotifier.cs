using LabFusion.Player;
using MashGamemodeLibrary.Entities.ECS.BaseComponents;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Interaction;

namespace Clockhunt.Entities.Tags;

public class ClockGrabNotifier : IComponent, IGrabCallback, IDropCallback
{
    public static readonly HashSet<PlayerID> Holders = new();

    public void OnDropped(GrabData grab)
    {
        if (grab.NetworkPlayer == null)
            return;
        
        Holders.Remove(grab.NetworkPlayer.PlayerID);
    }

    public void OnGrabbed(GrabData grab)
    {
        if (grab.NetworkPlayer == null)
            return;
        
        Holders.Add(grab.NetworkPlayer.PlayerID);
    }
}