using HarmonyLib;
using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Player.Data.Components.Colliders.Caches;

namespace MashGamemodeLibrary.Player.Data.Components.Colliders.Patches;

[HarmonyPatch(typeof(MarrowEntity))]
public class MarrowEntityPatches
{
    [HarmonyPatch(nameof(MarrowEntity.OnPoolSpawn))]
    [HarmonyPostfix]
    private static void OnPoolSpawn_Postfix(MarrowEntity __instance)
    {
        if (__instance == null)
            return;

        CachedColliderCache.OnMarrowEntityCreated(__instance);
    }

    [HarmonyPatch(nameof(MarrowEntity.Despawn))]
    [HarmonyPostfix]
    private static void Despawn_Postfix(MarrowEntity __instance)
    {
        if (__instance == null)
            return;

        CachedColliderCache.OnMarrowEntityDestroyed(__instance);
    }
}