using HarmonyLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Helpers;

namespace MashGamemodeLibrary.Player.Data.Extenders.Visibility.Patches;

[HarmonyPatch(typeof(PhysGrounder))]
public class PhysGrounderPatches
{
    [HarmonyPatch(nameof(PhysGrounder.HighFallClipSpawn))]
    [HarmonyPrefix]
    public static bool HighFallClipSpawnPrefix(PhysGrounder __instance)
    {
        if (__instance == null)
            return true;

        var rig = __instance.physRig?.manager;
        if (rig == null)
            return true;

        if (!NetworkPlayerManager.TryGetPlayer(rig, out var player))
            return true;

        // TODO: Change to IsHidden
        if (player.PlayerID.IsSpectating())
            return false;

        return true;
    }
}