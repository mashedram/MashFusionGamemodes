using HarmonyLib;
using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Grabbables;
using LabFusion.MonoBehaviours;
using LabFusion.Player;
using LabFusion.SDK.Extenders;
using LabFusion.Senders;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Vision;
using UnityEngine;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(Grip))]
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

        if (!PlayerGrabManager.CanGrabEntity(grab)) return false;
        
        item.Grip.ForceDetach();
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Hand), nameof(Hand.AttachObject))]
    public static bool AttachObject_Prefix(Hand __instance)
    {
        var grab = new GrabData(__instance);
        return PlayerGrabManager.CanGrabEntity(grab);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hand), nameof(Hand.AttachObject))]
    public static void AttachObject_Postfix(Hand __instance)
    {
        var grab = new GrabData(__instance);
        if (DropIfNeeded(grab)) return;
        PlayerGrabManager.OnGrab(grab);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hand), nameof(Hand.DetachObject))]
    public static void DetachObject_Postfix(Hand __instance)
    {
        var grab = new GrabData(__instance);
        PlayerGrabManager.OnDrop(grab);
    }
    
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(Grip), nameof(Grip.OnAttachedToHand))]
    // public static void OnAttachedToHand_Postfix(Grip __instance, Hand hand)
    // {
    //     if (!hand)
    //         return;
    //     
    //     if (DropIfNeeded(__instance, hand))
    //         return;
    //     
    //     PlayerHider.OnGrab(hand);
    //     __instance._marrowEntity?.OnGrab(hand);
    // }


    // [HarmonyPatch(typeof(Grip), nameof(Grip.OnDetachedFromHand))]
    // [HarmonyPostfix]
    // public static void OnDetachedFromHand_Postfix(Grip __instance, Hand hand)
    // {
    //     if (!hand)
    //         return;
    //     
    //     PlayerHider.OnDrop(hand);
    //     __instance._marrowEntity?.OnDrop(hand);
    // }

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