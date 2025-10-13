using HarmonyLib;
using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Grabbables;
using LabFusion.MonoBehaviours;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.SDK.Extenders;
using LabFusion.Senders;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Vision;
using UnityEngine;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch]
public class GripPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandHoverBegin))]
    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandHoverEnd))]
    [HarmonyPriority(10000)]
    public static bool InventoryGrabAttempt(InventorySlotReceiver __instance, Hand hand)
    {
        if (hand == null) return true;

        var grab = new GrabData(hand, __instance);
        return PlayerGrabManager.CanGrabEntity(grab);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandGrab))]
    [HarmonyPriority(10000)]
    public static bool InventoryGrabAttempt2(InventorySlotReceiver __instance, Hand hand)
    {
        if (hand == null) return true;
        var grab = new GrabData(hand, __instance);

        var res = PlayerGrabManager.CanGrabEntity(grab);
        if (!res)
            __instance.DropWeapon();

        return res;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InteractableIcon), nameof(InteractableIcon.MyFarHandHoverBegin))]
    [HarmonyPatch(typeof(InteractableIcon), nameof(InteractableIcon.MyHandHoverBegin))]
    [HarmonyPriority(10000)]
    public static bool IconAttempt(InteractableIcon __instance, Hand hand)
    {
        if (!hand || !__instance.m_Grip)
            return true;
        
        var grab = new GrabData(hand, __instance.m_Grip);

        return PlayerGrabManager.CanGrabEntity(grab);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.CoPull))]
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnStartAttach))]
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnFarHandHoverBegin))]
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnFarHandHoverUpdate))]
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnFarHandHoverEnd))]
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnForcePullComplete))]
    [HarmonyPriority(10000)]
    public static bool ForceGrabAttempt(Hand hand, ForcePullGrip __instance)
    {
        if (!hand || !__instance._grip)
            return true;

        var grab = new GrabData(hand, __instance._grip);

        return PlayerGrabManager.CanGrabEntity(grab);
    }

    private static bool DropIfNeeded(GrabData grab)
    {
        if (!grab.IsHoldingItem(out var item)) return false;

        if (PlayerGrabManager.CanGrabEntity(grab)) return false;
        
        item.Grip.ForceDetach();
        return true;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Hand), nameof(Hand.AttachObject))]
    [HarmonyPatch(typeof(Hand), nameof(Hand.AttachJoint))]
    [HarmonyPatch(typeof(Hand), nameof(Hand.AttachIgnoreBodyJoints))]
    [HarmonyPriority(10000)]
    public static bool GrabAttempt(Hand __instance, GameObject objectToAttach)
    {
        if (!objectToAttach)
            return true;

        var grab = new GrabData(__instance, objectToAttach);
        return PlayerGrabManager.CanGrabEntity(grab);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Grip), nameof(Grip.OnAttachedToHand))]
    public static void AttachObject_Postfix(Grip __instance, Hand hand)
    {
        if (hand == null)
            return;
        
        var grab = new GrabData(hand, __instance);

        if (DropIfNeeded(grab))
            return;
        
        PlayerGrabManager.OnGrab(grab);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Grip), nameof(Grip.OnDetachedFromHand))]
    public static void DetachObject(Grip __instance)
    {
        var hand = __instance.GetHand();
        if (hand == null)
            return;
        
        var grab = new GrabData(hand, __instance);
        PlayerGrabManager.OnDrop(grab);
    }
    
    // Other
    [HarmonyPatch(typeof(GrabHelper), nameof(GrabHelper.SendObjectAttach))]
    [HarmonyPrefix]
    public static bool SendObjectAttach_Prefix(Hand hand, Grip grip)
    {
        var grab = new GrabData(hand, grip);
        return PlayerGrabManager.CanGrabEntity(grab);
    }

    // TODO: Look into if this is needed
    // [HarmonyPatch(typeof(NetworkEntityManager), nameof(NetworkEntityManager.TransferOwnership))]
    // [HarmonyPrefix]
    // public static bool TransferOwnership_Prefix(NetworkEntity entity, PlayerID ownerID)
    // {
    //     var marrowEntity = entity.GetExtender<IMarrowEntityExtender>()?.MarrowEntity;
    //
    //     if (ownerID.IsMe)
    //         return !PlayerGrabManager.IsForceDisabled(entity, marrowEntity);
    //     
    //     return true;
    // }

    [HarmonyPatch(typeof(GrabHelper), nameof(GrabHelper.SendObjectForcePull))]
    [HarmonyPrefix]
    public static bool SendObjectForcePull_Prefix(Hand hand, Grip grip)
    {
        var grab = new GrabData(hand, grip);
        return PlayerGrabManager.CanGrabEntity(grab);
    }
}