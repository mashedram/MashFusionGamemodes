using HarmonyLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Stats;

namespace MashGamemodeLibrary.Patches;

// Gracious credit to notnotnotswipez and Hahoos for the source of these patches.
// Saved my ass so much time searching for the methods myself
[HarmonyPatch(typeof(Avatar))]
public static class AvatarPatches
{
    private static bool TryGetStatistics(Avatar instance, out AvatarStats avatarStats)
    {
        avatarStats = default!;
        if (instance == null)
            return false;
        
        if (instance == null || instance.name == "[RealHeptaRig (Marrow1)]")
            return false;
        
        var rigManager = instance.GetComponentInParent<RigManager>();
        if (rigManager == null)
            return false;

        if (rigManager != BoneLib.Player.RigManager)
            return false;

        if (!AvatarStatManager.TryGetLocalStats(instance, out avatarStats))
            return false;

        return true;
    }
    
    [HarmonyPatch(nameof(Avatar.ComputeBaseStats))]
    [HarmonyPostfix]
    public static void ComputeBaseStatsPostfix(Avatar __instance)
    {
        if (!TryGetStatistics(__instance, out var stats))
            return;

        __instance._speed = stats.Speed;
        __instance._agility = stats.Agility;
        __instance._strengthUpper = stats.UpperStrength;
        __instance._strengthGrip = stats.UpperStrength;
        __instance._strengthLower = stats.LowerStrength;
    }
}