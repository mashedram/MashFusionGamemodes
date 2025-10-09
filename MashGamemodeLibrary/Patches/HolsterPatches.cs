using HarmonyLib;
using Il2CppSLZ.Marrow;
using MashGamemodeLibrary.Vision;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(InventorySlotReceiver))]
public static class HolsterPatches
{
    [HarmonyPatch(nameof(InventorySlotReceiver.OnHandGrab))]
    [HarmonyPostfix]
    private static void OnHandGrab_Postfix(InventorySlotReceiver __instance, Hand hand)
    {
        PlayerHider.OnUnholster(__instance);
    }

    [HarmonyPatch(nameof(InventorySlotReceiver.OnHandDrop))]
    [HarmonyPostfix]
    private static void OnHandDrop_Postfix(InventorySlotReceiver __instance, IGrippable host)
    {
        PlayerHider.OnHolster(__instance);
    }

    [HarmonyPatch(typeof(InventoryAmmoReceiver), nameof(InventoryAmmoReceiver.UpdateAmmoArt))]
    [HarmonyPostfix]
    private static void UpdateAmmoArt_Postfix(InventoryAmmoReceiver __instance)
    {
        PlayerHider.UpdateAmmoHolster(__instance);
    }
}