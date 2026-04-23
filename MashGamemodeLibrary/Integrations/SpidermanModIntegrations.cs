using HarmonyLib;
using System.Reflection;
using MashGamemodeLibrary.Player.Helpers;
using Spiderman;

namespace MashGamemodeLibrary.Integrations;

public static class SpidermanModIntegrations
{
    private static class SpidermanModPatches
    {
        [HarmonyPrefix]
        public static bool WebShooter_CheckGestureInput_Prefix(WebShooter __instance)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (__instance == null)
                return true;

            // Only gets called locally
            if (CrippleHelper.IsCrippled)
                return false;

            return true;
        }
    }

    // Seperated to avoid DLL missing errors
    public static void TryPatch()
    {
        // Automatic patching does not work, so we have to do it manually with reflection
        try
        {
            // Create Harmony instance
            var harmony = new HarmonyLib.Harmony("com.mash.gamemodes.spidermanmod");

            // Get the CheckGestureInput method using reflection
            var webShooterType = typeof(WebShooter);
            var targetMethod = webShooterType.GetMethod("CheckGestureInput", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (targetMethod == null)
            {
                MelonLoader.MelonLogger.Warning("Spiderman integration: Could not find CheckGestureInput method");
                return;
            }

            // Get the prefix method from our patch class
            var patchesType = typeof(SpidermanModPatches);
            var patchMethod = patchesType.GetMethod(nameof(SpidermanModPatches.WebShooter_CheckGestureInput_Prefix));

            // Apply the patch using reflection-based method
            var result = harmony.Patch(targetMethod, prefix: new HarmonyMethod(patchMethod));
            
            if (result != null)
            {
                MelonLoader.MelonLogger.Msg("Spiderman integration: Successfully applied CheckGestureInput patch");
            }
            else
            {
                MelonLoader.MelonLogger.Warning("Spiderman integration: Failed to apply patch, harmony.Patch returned null");
            }
        }
        catch (Exception ex)
        {
            MelonLoader.MelonLogger.Error($"Failed to patch Spiderman integration: {ex}");
        }
    }
    
    public static void TryInitialize()
    {
        // Check if Spiderman mod is loaded
        var spidermanMod = MelonLoader.MelonBase.FindMelon("Spiderman", "notnotnotswipez");
        if (spidermanMod == null)
            return;

        TryPatch();
    }
}