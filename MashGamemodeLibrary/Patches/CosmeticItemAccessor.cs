using HarmonyLib;
using Il2CppSLZ.Marrow;
using LabFusion.SDK.Cosmetics;
using LabFusion.SDK.Points;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(PointItem))]
public static class CosmeticItemAccessor
{
    private static readonly List<Dictionary<RigManager, CosmeticInstance>> RegisteredCosmeticDictionaries = new();

    [HarmonyPatch(nameof(PointItem.OnRegistered))]
    [HarmonyPostfix]
    private static void OnRegistered_Postfix(PointItem __instance)
    {
        if (__instance is not CosmeticItem cosmeticItem)
            return;

        var cosmeticDictionary = Traverse.Create(cosmeticItem).Field<Dictionary<RigManager, CosmeticInstance>>("_accessoryInstances").Value;
        RegisteredCosmeticDictionaries.Add(cosmeticDictionary);
    }

    public static IEnumerable<CosmeticInstance> GetCosmetics(this RigManager rigManager)
    {
        return RegisteredCosmeticDictionaries
            .Select(d => d!.GetValueOrDefault(rigManager, null))
            .OfType<CosmeticInstance>();
    }
}