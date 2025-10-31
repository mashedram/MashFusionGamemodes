using HarmonyLib;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Entities.Tagging;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(NetworkEntity))]
public class NetworkEntityPatch
{
    [HarmonyPatch(nameof(NetworkEntity.Unregister))]
    [HarmonyPrefix]
    public static void Prefix(NetworkEntity __instance)
    {
        if (__instance == null)
            return;
        
        EntityTagManager.Remove(__instance.ID);
        PlayerGrabManager.Remove(__instance.ID);
    }
}