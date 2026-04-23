using System.Diagnostics;
using HarmonyLib;
using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Patches;

[HarmonyPatch(typeof(ImpactProperties))]
public class ImpactPropertiesPatches
{
    [HarmonyPatch(nameof(ImpactProperties.Awake))]
    [HarmonyPostfix]
    public static void AwakePostfix(ImpactProperties __instance)
    {
        try
        {
            if (__instance == null)
                return;
        
            if (__instance.transform == null)
                return;

            MarrowEntityEventHandler.FixColliderLayers(__instance.transform, 2);
        } catch (Exception e)
        {
            InternalLogger.Error($"An error occurred in ImpactPropertiesPatches.AwakePostfix: {e}");
            InternalLogger.Error("Stack Trace: " + new StackTrace());
        }
    }
}