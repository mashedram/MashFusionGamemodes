using HarmonyLib;
using Il2CppSLZ.Marrow.Interaction;
using MashGamemodeLibrary.Player.Data.Extenders.Colliders.Caches;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Data.Extenders.Colliders.Patches;

[HarmonyPatch(typeof(MarrowEntity))]
public class MarrowEntityPatches
{
    [HarmonyPatch(nameof(MarrowEntity.Awake))]
    [HarmonyPostfix]
    private static void Awake_Postfix(MarrowEntity __instance)
    {
        try
        {
            if (__instance == null)
                return;

            MarrowEntityEventHandler.OnMarrowEntityCreated(__instance);
        }
        catch (Exception e)
        {
            InternalLogger.Error($"An error occurred in MarrowEntityPatches.Awake_Postfix: {e}");
        }
    }
}