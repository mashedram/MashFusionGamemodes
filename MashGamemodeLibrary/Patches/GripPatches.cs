using HarmonyLib;
using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Grabbables;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Player.Spectating;
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
        if (!__instance || !hand) return true;

        var grab = new GrabData(hand, __instance);
        return PlayerGrabManager.CanGrabEntity(grab);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandGrab))]
    [HarmonyPriority(10000)]
    public static bool InventoryGrabAttempt2(InventorySlotReceiver __instance, Hand hand)
    {
        if (!__instance || !hand) return true;
        
        var grab = new GrabData(hand, __instance);

        if (PlayerGrabManager.CanGrabEntity(grab))
            return true;
           
        __instance.DropWeapon();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InteractableIcon), nameof(InteractableIcon.MyFarHandHoverBegin))]
    [HarmonyPatch(typeof(InteractableIcon), nameof(InteractableIcon.MyHandHoverBegin))]
    [HarmonyPriority(10000)]
    public static bool IconAttempt(InteractableIcon __instance, Hand hand)
    {
        if (!__instance || !hand)
            return true;
        if (!__instance.m_Grip)
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
        if (!__instance || !hand)
            return true;
        
        if (!__instance._grip)
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
        if (!__instance || !objectToAttach)
            return true;
        
        if (!objectToAttach)
            return true;

        var grab = new GrabData(__instance, objectToAttach);
        return PlayerGrabManager.CanGrabEntity(grab);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Grip), nameof(Grip.OnAttachedToHand))]
    public static void AttachObject_Postfix(Grip __instance, Hand hand)
    {
        if (__instance == null || hand == null)
            return;

        var grab = new GrabData(hand, __instance);

        if (DropIfNeeded(grab))
            return;

        PlayerGrabManager.OnGrab(grab);
    }

    // We need a prefix here,
    // because otherwise the grip will not contain the held item anymore and we can't check what it was
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Grip), nameof(Grip.OnDetachedFromHand))]
    public static void DetachObject(Grip __instance)
    {
        if (__instance == null)
            return;
        
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
        if (!hand || !grip)
            return true;
        
        var grab = new GrabData(hand, grip);
        return PlayerGrabManager.CanGrabEntity(grab);
    }

    [HarmonyPatch(typeof(NetworkEntityManager), nameof(NetworkEntityManager.TakeOwnership))]
    [HarmonyPrefix]
    public static bool TakeOwnership_Prefix(NetworkEntity entity)
    {
        return !SpectatorManager.IsLocalPlayerSpectating();
    }


    [HarmonyPatch(typeof(GrabHelper), nameof(GrabHelper.SendObjectForcePull))]
    [HarmonyPrefix]
    public static bool SendObjectForcePull_Prefix(Hand hand, Grip grip)
    {
        var grab = new GrabData(hand, grip);
        return PlayerGrabManager.CanGrabEntity(grab);
    }
}