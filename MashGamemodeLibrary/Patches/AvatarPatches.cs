using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using LabFusion.Utilities;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Player.Stats;

namespace MashGamemodeLibrary.Patches;

// Gracious credit to notnotnotswipez and Hahoos for the source of these patches.
// Saved my ass so much time searching for the methods myself
[HarmonyPatch(typeof(Avatar))]
public static class AvatarPatches
{
    private static Avatar? _currentLocalAvatar;
    private static AvatarStats? _currentStats;

    private static bool IsLocalAvatar(Avatar instance)
    {
        if (instance == null)
            return false;
        
        var rigManager = instance.GetComponentInParent<RigManager>();
        if (rigManager == null)
            return false;

        if (!rigManager.IsLocalPlayer())
            return false;

        return true;
    }
    
    private static void RefreshLocalAvatar(Avatar avatar)
    {
        if (!IsLocalAvatar(avatar))
            return;

        // Check if it's a loading avatar or the player controller avatar
        if (avatar.name == "[RealHeptaRig (Marrow1)]")
        {
            _currentLocalAvatar = null;
            _currentStats = null;
            return;
        }
        
        _currentLocalAvatar = avatar;
        _currentStats = AvatarStatManager.GetLocalStats(avatar);

    }
    
    private static bool TryGetStatistics(Avatar instance, out AvatarStats avatarStats)
    {
        avatarStats = default!;
        if (_currentStats == null || !instance.Equals(_currentLocalAvatar))
            return false;
        
        avatarStats = _currentStats.Value;
        return true;
    }
    
    [HarmonyPatch(nameof(Avatar.ComputeBaseStats))]
    [HarmonyPostfix]
    public static void ComputeBaseStatsPostfix(Avatar __instance)
    {
        RefreshLocalAvatar(__instance);
        if (!TryGetStatistics(__instance, out var stats))
            return;

        __instance._speed = stats.Speed;
        __instance._agility = stats.Agility;
        __instance._strengthUpper = stats.UpperStrength;
        __instance._strengthGrip = stats.UpperStrength;
        __instance._strengthLower = stats.LowerStrength;
    }
    
    [HarmonyPatch(nameof(Avatar.speed), MethodType.Getter)]
    [HarmonyPostfix]
    public static void SpeedGetterPostfix(Avatar __instance, ref float __result)
    {
        if (!TryGetStatistics(__instance, out var stats))
            return;

        __result = stats.Speed;
    }

    [HarmonyPatch(nameof(Avatar.agility), MethodType.Getter)]
    [HarmonyPostfix]
    public static void AgilityGetterPostfix(Avatar __instance, ref float __result)
    {
        if (!TryGetStatistics(__instance, out var stats))
            return;

        __result = stats.Agility;
    }

    [HarmonyPatch(nameof(Avatar.strengthUpper), MethodType.Getter)]
    [HarmonyPostfix]
    public static void StrengthUpperGetterPostfix(Avatar __instance, ref float __result)
    {
        if (!TryGetStatistics(__instance, out var stats))
            return;

        __result = stats.UpperStrength;
    }

    [HarmonyPatch(nameof(Avatar.strengthGrip), MethodType.Getter)]
    [HarmonyPostfix]
    public static void StrengthGripGetterPostfix(Avatar __instance, ref float __result)
    {
        if (!TryGetStatistics(__instance, out var stats))
            return;

        __result = stats.UpperStrength;
    }

    [HarmonyPatch(nameof(Avatar.strengthLower), MethodType.Getter)]
    [HarmonyPostfix]
    public static void StrengthLowerGetterPostfix(Avatar __instance, ref float __result)
    {
        if (!TryGetStatistics(__instance, out var stats))
            return;

        __result = stats.LowerStrength;
    }
}