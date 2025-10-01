using HarmonyLib;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Tagging;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(NetworkEntity))]
public class NetworkEntityPatch
{
    [HarmonyPatch(nameof(NetworkEntity.Unregister))]
    [HarmonyPrefix]
    public static void Prefix(NetworkEntity __instance)
    {
        EntityTagManager.Remove(__instance.ID);
    }
}