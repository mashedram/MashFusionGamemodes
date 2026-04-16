using HarmonyLib;
using LabFusion.SDK.Cosmetics;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Spectating;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(CosmeticInstance))]
public static class CosmeticInstancePatch
{
    [HarmonyPatch("OnApplyVisibility")]
    [HarmonyPrefix]
    private static bool CosmeticInstance_OnApplyVisibility_Prefix(CosmeticInstance __instance)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (__instance?.NetworkPlayer?.PlayerID == null)
            return true;

        if (!__instance.NetworkPlayer.IsSpectating())
            return true;

        __instance.accessory.SetActive(false);
        return false;
    }
}