using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Extenders;

namespace TheHunt.Utils;

public static class PlayerUtils
{
    public static void DropPlayers()
    {
        DropIfHoldingPlayer(BoneLib.Player.LeftHand);
        DropIfHoldingPlayer(BoneLib.Player.RightHand);
    }
    
    public static void DropIfHoldingPlayer(Hand hand)
    {
        var attached = hand.AttachedReceiver;

        if (attached.IsStatic)
        {
            hand.TryDetach();
            return;
        }
        
        var rb = attached?.Host?.Rb;
        if (rb == null) return;

        if (!MarrowBody.Cache.TryGet(rb.gameObject, out var body)) return;
        if (!MarrowBodyExtender.Cache.TryGet(body, out var entity)) return;

        var networkPlayer = entity.GetExtender<NetworkPlayer>();
        if (networkPlayer == null)
            return;
        
        hand.TryDetach();
    }
}