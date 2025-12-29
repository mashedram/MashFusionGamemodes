using HarmonyLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Visibility;
using MashGamemodeLibrary.Vision;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(RigArt))]
public class RigArtPatches
{
    [HarmonyPatch("ToggleAvatar")]
    [HarmonyPrefix]
    private static bool ToggleAvatar_Prefix(RigArt __instance)
    {
        var rig = Traverse.Create(__instance).Field<RigManager>("_rigManager").Value;
        if (rig == null)
            return true;

        if (!NetworkPlayer.RigCache.TryGet(rig, out var player))
            return true;

        return !player.PlayerID.IsHidden();
    }

    [HarmonyPatch("ToggleAmmoPouch")]
    [HarmonyPrefix]
    private static bool ToggleAmmoPouch_Prefix(RigArt __instance)
    {
        var rig = Traverse.Create(__instance).Field<RigManager>("_rigManager").Value;
        if (rig == null)
            return true;

        if (!NetworkPlayer.RigCache.TryGet(rig, out var player))
            return true;

        return !player.PlayerID.IsHidden();
    }
}