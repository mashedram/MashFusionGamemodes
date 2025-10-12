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
    [HarmonyPatch(typeof(Hand), nameof(Hand.AttachObject))]
    [HarmonyPriority(10000)]
    public static bool GrabAttempt(Hand __instance, GameObject objectToAttach)
    {
        if (!objectToAttach)
            return true;
        return !objectToAttach.TryGetComponent<MarrowEntity>(out var entity) || entity.CanGrabEntity(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandHoverBegin))]
    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandHoverEnd))]
    [HarmonyPriority(10000)]
    public static bool InventoryGrabAttempt(InventorySlotReceiver __instance, Hand hand)
    {
        if (!hand || __instance._weaponHost == null)
            return true;

        var entity = __instance._weaponHost?.GetGrip()._marrowEntity;
        return !entity || entity!.CanGrabEntity(hand);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandGrab))]
    [HarmonyPriority(10000)]
    public static bool InventoryGrabAttempt2(InventorySlotReceiver __instance, Hand hand)
    {
        if (!hand || __instance._weaponHost == null)
            return true;

        var entity = __instance._weaponHost.GetGrip()._marrowEntity;
        if (!entity)
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
        if (!hand || !__instance.m_Grip || !__instance.m_Grip._marrowEntity)
            return true;
        
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
        if (!hand || !__instance._grip || !__instance._grip._marrowEntity)
            return true;
        
        var entity = __instance._grip._marrowEntity;

        return !entity || entity.CanGrabEntity(hand);
    }

    private static bool DropIfNeeded(Grip? grip, Hand? hand)
    {
        
        var entity = grip?._marrowEntity;
        if (!entity)
            return false;

        if (!hand)
            return false;

        if (!NetworkPlayerManager.TryGetPlayer(hand!.manager, out var player))
            return false;
        
        if (!player.PlayerID.IsMe)
            return false;
        
        if (entity!.CanGrabEntity(player))
            return false;

        if (!grip!.HasHost)
            return false;

        if (!grip.Host.Rb) return false;

        grip.ForceDetach();
        return true;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Grip), nameof(Grip.OnAttachedToHand))]
    public static void OnAttachedToHand_Postfix(Grip __instance, Hand hand)
    {
        if (!hand)
            return;
        
        if (DropIfNeeded(__instance, hand))
            return;
        
        PlayerHider.OnGrab(hand);
        __instance._marrowEntity?.OnGrab(hand);
    }


    [HarmonyPatch(typeof(Grip), nameof(Grip.OnDetachedFromHand))]
    [HarmonyPostfix]
    public static void OnDetachedFromHand_Postfix(Grip __instance, Hand hand)
    {
        if (!hand)
            return;
        
        PlayerHider.OnDrop(hand);
        __instance._marrowEntity?.OnDrop(hand);
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
    public static bool TransferOwnership_Prefix(NetworkEntity entity, PlayerID ownerID)
    {
        var marrowEntity = entity.GetExtender<IMarrowEntityExtender>()?.MarrowEntity;

        if (ownerID.IsMe)
            return !PlayerGrabManager.IsForceDisabled(entity, marrowEntity);
        
        return true;
    }

    [HarmonyPatch(typeof(GrabHelper), nameof(GrabHelper.SendObjectForcePull))]
    [HarmonyPrefix]
    public static bool SendObjectForcePull_Prefix(Hand hand, Grip grip)
    {
        var entity = grip._marrowEntity;
        return !entity || entity.CanGrabEntity(hand);
    }
}