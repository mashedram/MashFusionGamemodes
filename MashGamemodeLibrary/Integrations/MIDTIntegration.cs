using System.Reflection;
using HarmonyLib;
using LabFusion.Network;
using LabFusion.Utilities;
using UnityEngine.Rendering;

namespace MashGamemodeLibrary.Integrations;

public static class MIDTIntegration
{
    private static class MIDTIntegrationPatches
    {
        [HarmonyPrefix]
        public static bool CanSpawnDevTools()
        {
            if (!NetworkInfo.HasServer)
                return true; 
            
            // Only gets called locally
            if (FusionDevTools.DevToolsDisabled)
                return false;

            return true;
        }
    }

    // Seperated to avoid DLL missing errors
    private static void TryPatch()
    {
        // Automatic patching does not work, so we have to do it manually with reflection
        try
        {
            // Create Harmony instance
            var harmony = new HarmonyLib.Harmony("com.mash.gamemodes.mitdintegration");

            // Get the CheckGestureInput method using reflection
            var webShooterType = typeof(MoreItemsInDevTools.Patches.AddDevMenuPatch);
            var targetMethod = webShooterType.GetMethod(
                nameof(MoreItemsInDevTools.Patches.AddDevMenuPatch.OnSpawnDelegateFusion), 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
            );
            
            if (targetMethod == null)
            {
                MelonLoader.MelonLogger.Warning("MIDT integration: Could not find OnSpawnDelegateFusion method");
                return;
            }

            // Get the prefix method from our patch class
            var patchesType = typeof(MIDTIntegrationPatches);
            var patchMethod = patchesType.GetMethod(nameof(MIDTIntegrationPatches.CanSpawnDevTools));

            // Apply the patch using reflection-based method
            var result = harmony.Patch(targetMethod, prefix: new HarmonyMethod(patchMethod));
            
            if (result != null)
            {
                MelonLoader.MelonLogger.Msg("MIDT integration: Successfully applied OnSpawnDelegateFusion patch");
            }
            else
            {
                MelonLoader.MelonLogger.Warning("MIDT integration: Failed to apply patch, harmony.Patch returned null");
            }
        }
        catch (Exception ex)
        {
            MelonLoader.MelonLogger.Error($"Failed to patch MIDT integration: {ex}");
        }
    }

    public static void TryInitialize()
    {
        // Check if Spiderman mod is loaded
        var spidermanMod = MelonLoader.MelonBase.FindMelon("More Items in Dev Tools", "doge15567");
        if (spidermanMod == null)
            return;
        
        TryPatch();
    }
}