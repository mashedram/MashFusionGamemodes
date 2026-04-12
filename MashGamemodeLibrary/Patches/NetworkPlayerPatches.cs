using HarmonyLib;
using LabFusion.Entities;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(NetworkPlayer))]
public class NetworkPlayerPatches
{
    [HarmonyPatch(nameof(NetworkPlayer.OnEntityCull))]
    [HarmonyPostfix]
    private static void OnEntityCull_Postfix(NetworkPlayer __instance, bool isInactive)
    {
        if (isInactive)
            return;

        // TODO: May not be needed anymore
    }
}