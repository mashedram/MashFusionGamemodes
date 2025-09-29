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
using UnityEngine;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(Grip))]
public class GripPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Hand), nameof(Hand.AttachObject))]
    [HarmonyPriority(10000)]
    public static bool GrabAttempt(Hand __instance, GameObject objectToAttach)
    {
        return !objectToAttach.TryGetComponent<MarrowEntity>(out var entity) || entity.CanGrabEntity(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandHoverBegin))]
    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandHoverEnd))]
    [HarmonyPriority(10000)]
    public static bool InventoryGrabAttempt(InventorySlotReceiver __instance, Hand hand)
    {
        if (hand == null || __instance._weaponHost == null)
            return true;

        var entity = __instance._weaponHost?.GetGrip()._marrowEntity;
        if (entity == null)
            return true;

        return !entity || entity.CanGrabEntity(hand);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandGrab))]
    [HarmonyPriority(10000)]
    public static bool InventoryGrabAttempt2(InventorySlotReceiver __instance, Hand hand)
    {
        if (hand == null || __instance._weaponHost == null)
            return true;

        var entity = __instance._weaponHost.GetGrip()._marrowEntity;
        if (entity == null)
            return true;

        var res = !entity || entity.CanGrabEntity(hand);
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
        var entity = __instance.m_Grip._marrowEntity;

        return !entity || entity.CanGrabEntity(hand);
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
        var entity = __instance._grip._marrowEntity;

        return !entity || entity.CanGrabEntity(hand);
    }
    
    
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Grip), nameof(Grip.OnAttachedToHand))]
    public static bool OnAttachedToHand_Prefix(Grip __instance, Hand hand)
    {
        __instance._marrowEntity.OnGrab(hand);
        return true;
    }

    [HarmonyPatch(typeof(Grip), nameof(Grip.OnDetachedFromHand))]
    [HarmonyPostfix]
    public static void OnDetachedFromHand_Postfix(Grip __instance, Hand hand)
    {
        __instance._marrowEntity.OnDrop(hand);
    }
    
    // Other
    [HarmonyPatch(typeof(GrabHelper), nameof(GrabHelper.SendObjectAttach))]
    [HarmonyPrefix]
    public static bool SendObjectAttach_Prefix(Hand hand, Grip grip)
    {
        var entity = grip._marrowEntity;

        return !entity || entity.CanGrabEntity(hand);
    }

    // TODO: Look into if this is needed
    [HarmonyPatch(typeof(NetworkEntityManager), nameof(NetworkEntityManager.TransferOwnership))]
    [HarmonyPrefix]
    public static bool TransferOwnership_Prefix(NetworkEntity entity, PlayerID newOwner)
    {
        return !PlayerGrabManager.IsForceDisabled();
    }

    [HarmonyPatch(typeof(CollisionSyncer), "OnCollisionEnter")]
    [HarmonyPrefix]
    public static bool OnCollisionEnter_Prefix(Collision collision)
    {
        return !PlayerGrabManager.IsForceDisabled();
    }

    [HarmonyPatch(typeof(GrabHelper), nameof(GrabHelper.SendObjectDetach))]
    [HarmonyPrefix]
    public static bool SendObjectDetach_Prefix(Hand hand, Grip grip)
    {
        var entity = grip._marrowEntity;
        return !entity || entity.CanGrabEntity(hand);
    }

    [HarmonyPatch(typeof(GrabHelper), nameof(GrabHelper.SendObjectForcePull))]
    [HarmonyPrefix]
    public static bool SendObjectForcePull_Prefix(Hand hand, Grip grip)
    {
        var entity = grip._marrowEntity;
        return !entity || entity.CanGrabEntity(hand);
    }
}