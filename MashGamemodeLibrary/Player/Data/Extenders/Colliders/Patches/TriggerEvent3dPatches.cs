using HarmonyLib;
using Il2CppUltEvents;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Patches;

[HarmonyPatch(typeof(TriggerEvents3D))]
public class TriggerEvent3dPatches
{
    [HarmonyPatch(nameof(TriggerEvents3D.OnTriggerEnter))]
    [HarmonyPrefix]
    public static bool OnTriggerEnterPrefix(TriggerEvents3D __instance, Collider collider)
    {
        if (__instance == null)
            return true;

        if (collider == null)
            return true;
        
        // INVALID MEMORY TIME BABY
        if (!GameObjectMemoryChecker.IsPointerAccessible(collider.m_CachedPtr))
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
        
        // INVALID MEMORY TIME BABY
        if (!GameObjectMemoryChecker.IsPointerAccessible(collider.m_CachedPtr))
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

        // INVALID MEMORY TIME BABY
        if (!GameObjectMemoryChecker.IsPointerAccessible(collider.m_CachedPtr))
            return true;
        
        if (collider.gameObject == null)
            return true;

        if (collider.gameObject.layer == CachedPhysicsRig.SpectatorLayer)
            return false;

        return true;
    }
}