using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction.Components;

namespace Clockhunt.Entities.Tags;

public class ClockGrabNotifier : IEntityGrabCallback, IEntityDropCallback
{
    public static readonly HashSet<PlayerID> Holders = new();

    public double DropCooldown => 0.05f;
    public double GrabCooldown => 0.05f;

    public void OnGrab(NetworkEntity entity, Hand hand)
    {
        if (!NetworkPlayerManager.TryGetPlayer(hand.manager, out var player))
            return;

        Holders.Add(player.PlayerID);
    }

    public void OnDrop(NetworkEntity networkEntity, Hand hand, MarrowEntity entity)
    {
        if (!NetworkPlayerManager.TryGetPlayer(hand.manager, out var player))
            return;

        Holders.Remove(player.PlayerID);
    }
}