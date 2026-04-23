using HarmonyLib;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppUltEvents;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Data;
using MashGamemodeLibrary.Util;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Patches;

[HarmonyPatch(typeof(TriggerEvents3D))]
public class TriggerEvent3dPatches
{
    
    private static ulong _maxSafePointerValue;
    private static ulong _maxCheckedPointerValue = ulong.MaxValue;
    /// <summary>
    /// Unity, in these events, can send pointers that point to internally garbage collected positions.
    /// We have no way to figure that out except for asking the OS directly, whic his expensive.
    ///
    /// This function approximates that. If we find a pointer we know is safe and exists in the programs address space, we accept it.
    /// We know that pointers that are invalid are always above 30927F74C9DE0000, testing has shown.
    /// 
    /// However, we don't want to keep checking every pointer against the OS, so we keep track of the highest pointer we've seen that is safe, and the lowest pointer we've seen that is unsafe, and if we get a pointer that is outside of those bounds, we can safely assume it's valid or invalid without asking the OS.
    /// </summary>
    /// <param name="intPtr"></param>
    /// <returns></returns>
    private static bool FastSafetyCheck(IntPtr intPtr)
    {
        var value = (ulong) intPtr.ToInt64();
        
        if (value <= _maxSafePointerValue)
            return true;
        
        if (value >= _maxCheckedPointerValue)
            return false;
        
        // If we get here, we might be in the danger zone, so we can do a more thorough check.
        var isSafe = GameObjectMemoryChecker.IsPointerAccessible(intPtr);
        if (isSafe)
        {
            // If it's safe, we can update our max safe pointer value to avoid future checks.
            _maxSafePointerValue = value;
            if (_maxSafePointerValue > _maxCheckedPointerValue)
            {
                // If our max safe pointer value is above our max checked pointer value, we can update our max checked pointer value to avoid future checks.
                _maxCheckedPointerValue = _maxSafePointerValue;
            }
        }
        else
        {
            if (value >= _maxCheckedPointerValue) 
                return isSafe;
            
            // If it's not safe, we can update our max checked pointer value to avoid future checks.
            _maxCheckedPointerValue = value;
            if (_maxCheckedPointerValue < _maxSafePointerValue)
            {
                // If our max checked pointer value is below our max safe pointer value, we can update our max safe pointer value to avoid future checks.
                _maxSafePointerValue = _maxCheckedPointerValue;
            }
        }
        
        return isSafe;
    }
    
    [HarmonyPatch(nameof(TriggerEvents3D.OnTriggerEnter))]
    [HarmonyPrefix]
    public static bool OnTriggerEnterPrefix(TriggerEvents3D __instance, Collider collider)
    {
        if (__instance == null)
            return true;

        if (collider == null)
            return true;
        
        if (!FastSafetyCheck(collider.m_CachedPtr))
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
        
        if (!FastSafetyCheck(collider.m_CachedPtr))
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
        
        if (!FastSafetyCheck(collider.m_CachedPtr))
            return true;
        
        if (collider.gameObject == null)
            return true;
        
        if (collider.gameObject.layer == CachedPhysicsRig.SpectatorLayer)
            return false;
            
        return true;
    }
}