using HarmonyLib;
using LabFusion.Network;
using LabFusion.RPC;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(NetworkAssetSpawner))]
public static class DevToolsPatches
{
    public static bool CanSpawn = true;
    
    [HarmonyPatch(nameof(NetworkAssetSpawner.Spawn))]
    [HarmonyPrefix]
    public static bool Prefix()
    {
        return NetworkInfo.IsHost || CanSpawn;
    }
}