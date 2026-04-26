using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppUltEvents;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Patches;

[HarmonyPatch(typeof(TriggerEvents3D))]
public class TriggerEvent3dPatches
{
    // For some reason the patches may get called with a pointer pointing to nothing, but not null, so we need to check if the object is valid first
    private static bool IsValid(Il2CppObjectBase collider)
    {
        if (collider.WasCollected)
            return false;
        
        var nestedTypeClassPointer = Il2CppClassPointerStore<Collider>.NativeClassPtr;
        if (nestedTypeClassPointer == IntPtr.Zero)
            return false;

        var ownClass = IL2CPP.il2cpp_object_get_class(collider.Pointer);
        return IL2CPP.il2cpp_class_is_assignable_from(nestedTypeClassPointer, ownClass);
    }
    
    [HarmonyPatch(nameof(TriggerEvents3D.OnTriggerEnter))]
    [HarmonyPrefix]
    public static bool OnTriggerEnterPrefix(TriggerEvents3D __instance, Collider collider)
    {
        if (__instance == null)
            return true;

        if (collider == null)
            return true;
        
        if (!IsValid(collider))
            return true;
        
        if (collider.gameObject == null)
            return true;

        if (collider.gameObject.layer == CachedPhysicsRig.SpectatorLayer)
            return false;

        return true;
    }

    [HarmonyPatch(nameof(TriggerEvents3D.OnTriggerExit))]
    [HarmonyPrefix]
    public static bool OnTriggerExitPrefix(TriggerEvents3D __instance, Collider collider)
    {
        if (__instance == null)
            return true;

        if (collider == null)
            return true;
        
        if (!IsValid(collider))
            return true;
        
        if (collider.gameObject == null)
            return true;

        if (collider.gameObject.layer == CachedPhysicsRig.SpectatorLayer)
            return false;

        return true;
    }

    [HarmonyPatch(nameof(TriggerEvents3D.OnTriggerStay))]
    [HarmonyPrefix]
    public static bool OnTriggerStayPrefix(TriggerEvents3D __instance, Collider collider)
    {
        if (__instance == null)
            return true;
    
        if (collider == null)
            return true;
        
        if (!IsValid(collider))
            return true;
        
        if (collider.gameObject == null)
            return true;
        
        if (collider.gameObject.layer == CachedPhysicsRig.SpectatorLayer)
            return false;
            
        return true;
    }
}