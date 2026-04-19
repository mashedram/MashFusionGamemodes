using HarmonyLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Network;

namespace TheHunt.Player.Inventory.Patches;

[HarmonyPatch(typeof(LabFusion.Marrow.Patching.InventoryAmmoReceiverPatches))]
public class InventoryAmmoReceiverPatches
{
    [HarmonyPatch(nameof(LabFusion.Marrow.Patching.InventoryAmmoReceiverPatches.OnHandGrabPrefix))]
    [HarmonyPrefix]
    public static bool OnHandGrab([HarmonyArgument(0)] InventoryAmmoReceiver instance)
    {
        if (!NetworkInfo.HasServer)
            return true;
        
        // Only act on the local player
        if (!NetworkPlayerManager.TryGetPlayer(instance._parentRigManager, out var player) || !player.PlayerID.IsMe)
            return true;

        if (!LocalAmmoManager.HasAmmo()) 
            return false;
        
        LocalAmmoManager.IncrementAmmo();
        return true;
    }
}