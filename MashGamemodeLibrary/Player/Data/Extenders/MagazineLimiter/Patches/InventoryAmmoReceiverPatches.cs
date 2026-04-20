using HarmonyLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Network;

namespace MashGamemodeLibrary.Player.Data.Extenders.MagazineLimiter.Patches;

[HarmonyPatch(typeof(LabFusion.Marrow.Patching.InventoryAmmoReceiverPatches))]
public class InventoryAmmoReceiverPatches
{
    [HarmonyPatch(nameof(LabFusion.Marrow.Patching.InventoryAmmoReceiverPatches.OnHandGrabPrefix))]
    [HarmonyPrefix]
    public static bool OnHandGrab([HarmonyArgument(0)] InventoryAmmoReceiver instance)
    {
        if (!NetworkInfo.HasServer)
            return true;
        
        // Check the magazine first
        if (instance._activeMagazines.Count == 0)
            return true;

        var magazine = instance._activeMagazines[0];
        if (magazine?.magazineState == null)
            return true;
        
        // Only act on the local player
        if (!NetworkPlayerManager.TryGetPlayer(instance._parentRigManager, out var player) || !player.PlayerID.IsMe)
            return true;

        // Get the local ammo limiter
        var ammoLimiter = PlayerDataManager.GetLocalPlayerData()?.GetExtender<AmmunitionLimiterExtender>();
        if (ammoLimiter == null)
            return true;

        if (!ammoLimiter.CanUseMagazine())
            return false;
        
        ammoLimiter.UseMagazine(magazine.magazineState.AmmoCount);    
        return true;
    }
}