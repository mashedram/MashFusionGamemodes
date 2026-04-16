using HarmonyLib;
using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.networking.Compatiblity;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(GamemodeRegistration))]
public class GamemodeRegistryPatch
{
    [HarmonyPatch(nameof(GamemodeRegistration.RegisterGamemode), typeof(Type))]
    [HarmonyPostfix]
    private static void RegisterGamemode_Postfix(Type type)
    {
        var gamemode = GamemodeRegistration.Gamemodes.First(v => v.GetType() == type);
        GamemodeCompatibilityChecker.RegisterGamemodeInfo(gamemode);
    }
}