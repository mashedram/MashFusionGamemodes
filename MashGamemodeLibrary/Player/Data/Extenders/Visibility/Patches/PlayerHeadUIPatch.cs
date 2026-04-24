using HarmonyLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using MashGamemodeLibrary.Player.Helpers;

namespace MashGamemodeLibrary.Player.Data.Extenders.Visibility.Patches;

[HarmonyPatch(typeof(RigHeadUI))]
public static class PlayerHeadUIPatch
{
    private static readonly Dictionary<RigHeadUI, NetworkPlayer> HeadUIPlayerCache = new();
    
    [HarmonyPatch(nameof(RigHeadUI.Visible), MethodType.Setter)]
    [HarmonyPrefix]
    public static void Visible_Setter_Prefix(RigHeadUI __instance, ref bool value)
    {
        if (!HeadUIPlayerCache.TryGetValue(__instance, out var player))
            return;

        if (!player.PlayerID.IsValid)
        {
            HeadUIPlayerCache.Remove(__instance);
            return;
        }
        
        if (!PlayerDataManager.TryGetPlayerData(player.PlayerID, out var data)) 
            return;
        
        if (data.GetExtender<PlayerVisibility>().NametagVisible)
            return;
        
        value = false;
    }
    
    [HarmonyPatch(nameof(RigHeadUI.UpdateTransform))]
    [HarmonyPostfix]
    public static void UpdateTransform_Postfix(RigHeadUI __instance, RigManager rigManager)
    {
        if (!NetworkPlayerManager.TryGetPlayer(rigManager, out var player))
            return;

        HeadUIPlayerCache[__instance] = player;
    }

    [HarmonyPatch(nameof(RigHeadUI.Despawn))]
    [HarmonyPostfix]
    public static void Despawn_Postfix(RigHeadUI __instance)
    {
        HeadUIPlayerCache.Remove(__instance);
    }
}