using HarmonyLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using MashGamemodeLibrary.Player.Stats;

namespace MashGamemodeLibrary.Patches;

// Gracious credit to notnotnotswipez and Hahoos for the source code of these patches.
// Saved my ass so much time.
[HarmonyPatch(typeof(Avatar))]
public static class AvatarPatches
{
    [HarmonyPatch(nameof(Avatar.ComputeBaseStats))]
    [HarmonyPostfix]
    public static void ComputeBaseStatsPostfix(Avatar __instance)
    {
        var maybeStats = PlayerStatManager.LocalStatOverride;

        if (!maybeStats.HasValue)
            return;

        if (__instance == null || __instance.name == "[RealHeptaRig (Marrow1)]")
            return;

        var rigManager = __instance.GetComponentInParent<RigManager>();
        if (rigManager == null) return;

        if (rigManager != BoneLib.Player.RigManager)
            return;

        var stats = maybeStats.Value;
        __instance._speed = stats.Speed;
        __instance._agility = stats.Agility;
        __instance._strengthUpper = stats.UpperStrength;
        __instance._strengthGrip = stats.UpperStrength;
    }
}