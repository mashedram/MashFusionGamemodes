using HarmonyLib;
using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Entities.Interaction;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(Gun))]
public static class GunPatches
{
    [HarmonyPatch(nameof(Gun.OnFire))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void OnFire_Postfix(Gun __instance)
    {
        PlayerGunManager.InvokeOnGunFired(__instance);
    }
}