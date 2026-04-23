using HarmonyLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Helpers;

namespace MashGamemodeLibrary.Player.Data.Extenders.Visibility.Patches;

[HarmonyPatch(typeof(HeadSFX))]
public static class HeadSfxPatches
{
    private static bool CanPlay(HeadSFX headSfx)
    {
        if (headSfx == null)
            return true;

        var rig = headSfx._physRig?.manager;
        if (rig == null)
            return true;

        if (!NetworkPlayerManager.TryGetPlayer(rig, out var player))
            return true;

        // TODO: Change to IsHidden
        if (player.PlayerID.IsSpectating())
            return false;

        return true;
    }

    [HarmonyPatch(nameof(HeadSFX.Speak))]
    [HarmonyPrefix]
    public static bool SpeakPrefix(HeadSFX __instance)
    {
        return CanPlay(__instance);
    }

    [HarmonyPatch(nameof(HeadSFX.JumpEffort))]
    [HarmonyPrefix]
    public static bool JumpEffort(HeadSFX __instance)
    {
        return CanPlay(__instance);
    }

    [HarmonyPatch(nameof(HeadSFX.DoubleJump))]
    [HarmonyPrefix]
    public static bool DoubleJump(HeadSFX __instance)
    {
        return CanPlay(__instance);
    }

    [HarmonyPatch(nameof(HeadSFX.DyingVocal))]
    [HarmonyPrefix]
    public static bool DyingVocal(HeadSFX __instance)
    {
        return CanPlay(__instance);
    }

    [HarmonyPatch(nameof(HeadSFX.DeathVocal))]
    [HarmonyPrefix]
    public static bool DeathVocal(HeadSFX __instance)
    {
        return CanPlay(__instance);
    }

    [HarmonyPatch(nameof(HeadSFX.RecoveryVocal))]
    [HarmonyPrefix]
    public static bool RecoveryVocal(HeadSFX __instance)
    {
        return CanPlay(__instance);
    }

    [HarmonyPatch(nameof(HeadSFX.BigDamageVocal))]
    [HarmonyPrefix]
    public static bool BigDamageVocal(HeadSFX __instance)
    {
        return CanPlay(__instance);
    }

    [HarmonyPatch(nameof(HeadSFX.SmallDamageVocal))]
    [HarmonyPrefix]
    public static bool SmallDamageVocal(HeadSFX __instance)
    {
        return CanPlay(__instance);
    }
}