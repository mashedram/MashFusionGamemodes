using HarmonyLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Player.Helpers;

namespace MashGamemodeLibrary.Player.Data.Extenders.Visibility.Patches;

[HarmonyPatch(typeof(RigArt))]
public class RigArtPatches
{
    // TODO: Make this a postfix where we overwrite the end value
    [HarmonyPatch("ToggleAvatar")]
    [HarmonyPrefix]
    private static bool ToggleAvatar_Prefix(RigArt __instance)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (__instance == null)
            return true;

        // When local is spectating, allow the zoning to just do zoning
        if (SpectatorExtender.IsLocalPlayerSpectating())
            return true;
        
        var rig = Traverse.Create(__instance).Field<RigManager>("_rigManager").Value;
        if (rig == null)
            return true;

        if (!NetworkPlayer.RigCache.TryGet(rig, out var player))
            return true;
        
        // If the target is spectating, but we aren't, don't allow the avatar to be shown
        if (player.IsSpectating())
            return false;

        return true;
    }

    [HarmonyPatch("ToggleAmmoPouch")]
    [HarmonyPrefix]
    private static bool ToggleAmmoPouch_Prefix(RigArt __instance)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (__instance == null)
            return true;
        
        if (SpectatorExtender.IsLocalPlayerSpectating())
        {
            return true;
        }

        var rig = Traverse.Create(__instance).Field<RigManager>("_rigManager").Value;
        if (rig == null)
            return true;

        if (!NetworkPlayer.RigCache.TryGet(rig, out var player))
            return true;

        // TODO: Turn back to ishidden at some point
        return !player.PlayerID.IsSpectating();
    }
}