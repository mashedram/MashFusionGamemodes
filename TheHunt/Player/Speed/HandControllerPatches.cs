using HarmonyLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;

namespace TheHunt.Player.Speed;

[HarmonyPatch(typeof(OpenController))]
public static class HandControllerPatches
{
    [HarmonyPatch(nameof(OpenController.GetThumbStickAxis))]
    [HarmonyPostfix]
    public static void GetThumbStickAxisPostfix(OpenController __instance, ref UnityEngine.Vector2 __result)
    { 
        if (__instance == null)
            return;
        
        if (__instance.handedness != Handedness.LEFT)
            return;
            
        __result *= LocalSpeed.SpeedModifier;
    }
}