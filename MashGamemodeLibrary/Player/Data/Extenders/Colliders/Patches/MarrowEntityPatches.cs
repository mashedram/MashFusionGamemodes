using HarmonyLib;
using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Patches;

[HarmonyPatch(typeof(MarrowEntity))]
public class MarrowEntityPatches
{
    [HarmonyPatch(nameof(MarrowEntity.Awake))]
    [HarmonyPostfix]
    private static void Awake_Postfix(MarrowEntity __instance)
    {
        if (__instance == null)
            return;

        MarrowEntityEventHandler.OnMarrowEntityCreated(__instance);
    }

    [HarmonyPatch(nameof(MarrowEntity.OnPoolSpawn))]
    [HarmonyPostfix]
    private static void OnPoolSpawn_Postfix(MarrowEntity __instance)
    {
        if (__instance == null)
            return;

        MarrowEntityEventHandler.OnMarrowEntityCreated(__instance);
    }

    [HarmonyPatch(nameof(MarrowEntity.Despawn))]
    [HarmonyPostfix]
    private static void Despawn_Postfix(MarrowEntity __instance)
    {
        if (__instance == null)
            return;

        MarrowEntityEventHandler.OnMarrowEntityDestroyed(__instance);
    }
}