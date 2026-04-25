using HarmonyLib;
using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Grabbables;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Grabbing;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Util;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch]
public class GripPatches
{
    // Fix for spectator grabbing shenangians
    // TODO : This should technically, along with the one for force pull, be the only one we need
    [HarmonyPatch(typeof(InventoryHand), nameof(InventoryHand.OnOverlapEnter))]
    [HarmonyPrefix]
    public static bool OnOverlapEnter(InventoryHand __instance, GameObject other)
    {
        if (__instance == null || other == null)
            return true;
        
        if (!Grip.Cache.TryGet(other, out var grip))
            return true;
        
        if (SpectatorExtender.IsLocalPlayerSpectating())
            return false;
        
        var request = new GrabRequest(__instance, grip);
        if (!request.CanGrab())
            return false;

        return true;
    }

    [HarmonyPatch(typeof(Hand), nameof(Hand.AttachObject))]
    [HarmonyPatch(typeof(Hand), nameof(Hand.AttachJoint))]
    [HarmonyPatch(typeof(Hand), nameof(Hand.AttachIgnoreBodyJoints))]
    [HarmonyPrefix]
    public static bool AttachObject_Prefix(Hand __instance, GameObject objectToAttach)
    {
        if (__instance == null || objectToAttach == null)
            return true;
        
        if (!Grip.Cache.TryGet(objectToAttach, out var grip))
            return true;
        
        if (SpectatorExtender.IsLocalPlayerSpectating())
            return false;
        
        var request = new GrabRequest(__instance, grip);
        if (!request.CanGrab())
            return false;

        return true;
    }
    
    // Inventory
    
    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandHoverBegin))]
    [HarmonyPrefix]
    [HarmonyPriority(10000)]
    public static bool HandHoverBegin_Prefix(InventorySlotReceiver __instance, Hand hand)
    {
        if (__instance == null || hand == null) 
            return true;

        var host = __instance._weaponHost?.TryCast<InteractableHost>();
        if (host == null) 
            return true;
        
        if (host._grips.Count == 0)
            return true;
        
        var grab = new GrabRequest(hand, host._grips[0]);

        return grab.CanGrab();
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventorySlotReceiver), nameof(InventorySlotReceiver.OnHandGrab))]
    [HarmonyPriority(10000)]
    public static bool InventoryGrabAttempt(InventorySlotReceiver __instance, Hand hand)
    {
        if (__instance == null || hand == null) 
            return true;

        var host = __instance._weaponHost?.TryCast<InteractableHost>();
        if (host == null) 
            return true;
        
        if (host._grips.Count == 0)
            return true;
        
        var grab = new GrabRequest(hand, host._grips[0]);

        return grab.CanGrab();
    }

    // Forcepull
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(InteractableIcon), nameof(InteractableIcon.MyFarHandHoverBegin))]
    [HarmonyPatch(typeof(InteractableIcon), nameof(InteractableIcon.MyHandHoverBegin))]
    [HarmonyPriority(10000)]
    public static bool IconAttempt(InteractableIcon __instance, Hand hand)
    {
        if (__instance == null || hand == null)
            return true;

        var grip = __instance.m_Grip;
        if (grip == null)
            return true;

        var grab = new GrabRequest(hand, grip);

        return grab.CanGrab();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.CoPull))]
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnStartAttach))]
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnFarHandHoverBegin))]
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnFarHandHoverUpdate))]
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnFarHandHoverEnd))]
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnForcePullComplete))]
    [HarmonyPriority(10000)]
    public static bool ForceGrabAttempt(ForcePullGrip __instance, Hand hand)
    {
        if (__instance == null || hand == null)
            return true;

        var grip = __instance._grip;
        if (grip == null)
            return true;

        var grab = new GrabRequest(hand, grip);

        return grab.CanGrab();
    }

    // Events
    
    [HarmonyPatch(typeof(Grip), nameof(Grip.OnAttachedToHand))]
    [HarmonyPostfix]
    public static void OnAttachedToHand_Postfix(Grip __instance, Hand hand)
    {
        if (__instance == null || hand == null)
            return;
        
        try
        {
            var grab = new GrabRequest(hand, __instance);
            PlayerGrabManager.OnGrab(grab);
        }
        catch (Exception e)
        {
            MelonLogger.Error("Failed to postfix grab", e);
        }
    }
    
    
    [HarmonyPatch(typeof(Grip), nameof(Grip.OnDetachedFromHand))]
    [HarmonyPostfix]
    public static void OnDetachFromHand(Grip __instance, Hand hand)
    {
        if (__instance == null || hand == null)
            return;
    
        try
        {
            var grab = new GrabRequest(hand, __instance);
            PlayerGrabManager.OnDrop(grab);
        }
        catch (Exception e)
        {
            MelonLogger.Error("Failed to prefix drop", e);
        }
    }
}