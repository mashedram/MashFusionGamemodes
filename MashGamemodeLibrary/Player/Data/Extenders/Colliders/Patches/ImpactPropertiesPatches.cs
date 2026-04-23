using System.Diagnostics;
using HarmonyLib;
using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Patches;

[HarmonyPatch(typeof(ImpactProperties))]
public class ImpactPropertiesPatches
{
    [HarmonyPatch(nameof(ImpactProperties.Awake))]
    [HarmonyPostfix]
    public static void AwakePostfix(ImpactProperties __instance)
    {
        if (__instance == null)
            return;
        
        if (__instance.transform == null)
            return;

        MarrowEntityEventHandler.FixColliderLayers(__instance.transform, 2);
    }
}