using HarmonyLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Spectating;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(RigArt))]
public class RigArtPatches
{
    [HarmonyPatch("ToggleAmmoPouch")]
    [HarmonyPrefix]
    private static bool ToggleAmmoPouch_Prefix(RigArt __instance)
    {
        var rig = Traverse.Create(__instance).Field<RigManager>("_rigManager").Value;
        if (rig == null)
            return true;

        if (!NetworkPlayer.RigCache.TryGet(rig, out var player))
            return true;

        return !player.PlayerID.IsSpectating();
    }
}