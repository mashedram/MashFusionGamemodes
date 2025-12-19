using HarmonyLib;
using LabFusion.Entities;
using MashGamemodeLibrary.Vision;

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

        PlayerHider.Refresh(__instance.PlayerID);
    }
}