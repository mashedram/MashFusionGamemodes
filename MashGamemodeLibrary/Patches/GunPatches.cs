using HarmonyLib;
using Il2CppSLZ.Combat;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Combat;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Interaction.Attacking;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(Gun))]
public static class GunPatches
{
    [HarmonyPatch(nameof(Gun.Fire))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void Fire_Postfix(Gun __instance)
    {
        if (__instance == null)
            return;

        PlayerGunManager.InvokeGunFired(__instance);
    }

    [HarmonyPatch(nameof(Gun.OnTriggerGripAttached))]
    [HarmonyPostfix]
    private static void OnGripAttached_Postfix(Gun __instance)
    {
        if (__instance == null)
            return;

        PlayerGunManager.OnGunGrabbed(__instance);
    }
    
    [HarmonyPatch(typeof(GenericAttackReceiver), nameof(GenericAttackReceiver.ReceiveAttack))]
    [HarmonyPrefix] 
    private static bool ReceiveAttack_Postfix(GenericAttackReceiver __instance, Attack attack)
    {
        if (__instance == null )
            return true;

        AttackManager.ReceiveAttack(__instance, attack);

        // Prevent the patch from capturing an error here
        if (__instance.AttackEvent == null)
            return false;

        return true;
    }
}