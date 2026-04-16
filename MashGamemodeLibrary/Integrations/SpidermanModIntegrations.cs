using HarmonyLib;
using MashGamemodeLibrary.Player.Helpers;
using Spiderman;

namespace MashGamemodeLibrary.Integrations;

public static class SpidermanModIntegrations
{
    private static class SpidermanModPatches
    {

        [HarmonyPatch(typeof(WebShooter), "CheckGestureInput")]
        [HarmonyPrefix]
        public static bool WebShooter_CanShootWeb_Prefix(WebShooter __instance)
        {
            // Check it regardless
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (__instance == null)
                return true;

            // Only gets called locally
            if (SpectatorExtender.IsLocalPlayerSpectating())
                return false;

            return true;
        }
    }

    public static void TryInitialize()
    {
        // Check if Spiderman mod is loaded
        var spidermanMod = MelonLoader.MelonBase.FindMelon("Spiderman", "notnotnotswipez");
        if (spidermanMod == null)
            return;

        try
        {
            // Create Harmony instance
            var harmony = new HarmonyLib.Harmony("com.mash.gamemodes.spidermanmod");

            // Only patch the SpidermanModPatches class, not the entire assembly
            harmony.CreateClassProcessor(typeof(SpidermanModPatches)).Patch();
        }
        catch (Exception ex)
        {
            MelonLoader.MelonLogger.Error($"Failed to patch Spiderman integration: {ex}");
        }
    }
}