using HarmonyLib;
using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Entities.Interaction;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(Gun))]
public static class GunPatches
{
    [HarmonyPatch(nameof(Gun.Fire))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void Fire_Postfix(Gun __instance)
    {
        if (__instance == null)
            return;

        PlayerGunManager.InvokeGunFired(__instance);
    }

    [HarmonyPatch(nameof(Gun.OnTriggerGripAttached))]
    [HarmonyPostfix]
    private static void OnGripAttached_Postfix(Gun __instance)
    {
        if (__instance == null)
            return;

        PlayerGunManager.OnGunGrabbed(__instance);
    }
}