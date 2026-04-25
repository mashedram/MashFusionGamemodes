using HarmonyLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.Utilities;

namespace MashGamemodeLibrary.Player.Data.Extenders.MagazineLimiter.Patches;

[HarmonyPatch(typeof(LabFusion.Marrow.Patching.InventoryAmmoReceiverPatches))]
public class InventoryAmmoReceiverPatches
{
    [HarmonyPatch(nameof(LabFusion.Marrow.Patching.InventoryAmmoReceiverPatches.OnHandGrabPrefix))]
    [HarmonyPrefix]
    public static bool OnHandGrab([HarmonyArgument(0)] InventoryAmmoReceiver instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked || !instance._parentRigManager.IsLocalPlayer())
            return true;

        var magazine = instance._selectedMagazineData;
        if (magazine == null)
            return true;
        
        // Only act on the local player
        if (!NetworkPlayerManager.TryGetPlayer(instance._parentRigManager, out var player) || !player.PlayerID.IsMe)
            return true;

        // Reject if we have no actual ammo
        var ammoInventory = AmmoInventory.Instance;
        if (ammoInventory == null || ammoInventory.GetCartridgeCount(instance._selectedCartridgeData) <= 0)
            return true;

        // Get the local ammo limiter
        var ammoLimiter = PlayerDataManager.GetLocalPlayerData()?.GetExtender<AmmunitionLimiterExtender>();
        if (ammoLimiter == null)
            return true;

        if (!ammoLimiter.CanUseMagazine())
            return false;
        
        ammoLimiter.UseMagazine(magazine.rounds);    
        return true;
    }

    [HarmonyPatch(nameof(LabFusion.Marrow.Patching.InventoryAmmoReceiverPatches.OnHandDropPrefix))]
    [HarmonyPostfix]
    public static void OnHandDrop([HarmonyArgument(0)] InventoryAmmoReceiver instance, IGrippable host, bool __result)
    {
        if (!NetworkSceneManager.IsLevelNetworked || !instance._parentRigManager.IsLocalPlayer())
            return;

        var source = host.TryCast<InteractableHost>();
        if (source == null || !InteractableHostExtender.Cache.TryGet(source, out var networkEntity) || networkEntity.IsRegistered)
            return;
        
        var extender = networkEntity.GetExtender<MagazineExtender>();
        if (extender == null)
            return;
        
        // Only act on the local player
        if (!NetworkPlayerManager.TryGetPlayer(instance._parentRigManager, out var player) || !player.PlayerID.IsMe)
            return;
        
        // Only check we need to do
        if (extender.Component.magazinePlug._isLocked)
            return;
        
        // Get the local ammo limiter
        var ammoLimiter = PlayerDataManager.GetLocalPlayerData()?.GetExtender<AmmunitionLimiterExtender>();

        // Refund the unconsumed ammo
        ammoLimiter?.UseMagazine(-extender.Component.magazineState.AmmoCount);
    }
}