using HarmonyLib;
using Il2CppSLZ.Marrow;

namespace TheHunt.Player.Speed;

// TODO: I want to play the game now thank you
// [HarmonyPatch(typeof(OpenController))]
// public static class HandControllerPatches
// {
//     public static float Speed => 0.5f;
//     
//     [HarmonyPatch(nameof(OpenController.GetThumbStickAxis))]
//     [HarmonyPostfix]
//     public static void GetThumbStickAxisPostfix(OpenController __instance, ref UnityEngine.Vector2 __result)
//     { 
//         __result *= Speed;
//     }
// }