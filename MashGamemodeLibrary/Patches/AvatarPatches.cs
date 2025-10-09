using HarmonyLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Player;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(Avatar))]
public static class AvatarPatches
{
    [HarmonyPatch(nameof(Avatar.ComputeBaseStats))]
    [HarmonyPostfix]
    public static void ComputeBaseStatsPostfix(Avatar __instance)
    {
        var stats = PlayerStatManager.LocalStatOverride;

        if (stats == null)
            return;
        
        if (!__instance || __instance.name == "[RealHeptaRig (Marrow1)]")
            return;

        var rigManager = __instance.GetComponentInParent<RigManager>();
        if (!rigManager) return;
        
        if (rigManager != BoneLib.Player.RigManager)
            return;
        
        __instance._speed = stats.Speed;
        __instance._agility = stats.Agility;
        __instance._strengthUpper = stats.UpperStrength;
        __instance._strengthGrip = stats.UpperStrength;
    }
}