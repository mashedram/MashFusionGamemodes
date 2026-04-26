using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.Utilities;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Data.Extenders.MagazineLimiter.Patches;

[HarmonyPatch(typeof(LabFusion.Marrow.Patching.InventoryAmmoReceiverPatches))]
public class InventoryAmmoReceiverPatches
{
    const int MinimumAmmoConsumption = 4;

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
        
        // Consume the ammo with the minimum takes per mag, accounts for shotguns with a lot of ammo
        ammoLimiter.UseMagazine(Math.Max(magazine.rounds, MinimumAmmoConsumption));    
        return true;
    }

    [HarmonyPatch(nameof(LabFusion.Marrow.Patching.InventoryAmmoReceiverPatches.OnHandDropPrefix))]
    [HarmonyPostfix]
    public static void OnHandDrop([HarmonyArgument(0)] InventoryAmmoReceiver instance, IGrippable host, bool __result)
    {
        if (!NetworkSceneManager.IsLevelNetworked || !instance._parentRigManager.IsLocalPlayer())
            return;

        var source = host.TryCast<InteractableHost>();
        if (source == null)
            return;
        
        if (!InteractableHostExtender.Cache.TryGet(source, out var networkEntity)) 
            return;
        
        if (!networkEntity.IsRegistered)
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

        var fullMagazineCount = extender.Component.magazineState.magazineData.rounds;
        var magazineContent = extender.Component.magazineState.AmmoCount;
        // If the max magazine size is below the minimum ammo consumption, we need to refund the difference to prevent infinite ammo exploits with small magazines
        if (fullMagazineCount < MinimumAmmoConsumption)
        {
            // Refund the minimum ammo consumption, minus the actual used ammo
            var usedAmmo = fullMagazineCount - magazineContent;
            ammoLimiter?.UseMagazine(-(MinimumAmmoConsumption - usedAmmo));
        }
        else
        {
            // Refund the unconsumed ammo
            ammoLimiter?.UseMagazine(-magazineContent);
        }
    }
}