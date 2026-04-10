using HarmonyLib;
using Il2CppSLZ.Marrow;
using LabFusion.Entities;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Spectating;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(WindBuffetSFX))]
public class WindBuffetSfxPatches
{
    private static readonly Dictionary<WindBuffetSFX, PlayerID> WindBuffetSfxInstances = new();
    
    [HarmonyPatch(nameof(WindBuffetSFX.Awake))]
    [HarmonyPostfix]
    public static void AwakePostfix(WindBuffetSFX __instance)
    {
        if (__instance == null)
            return;

        var rigManager = __instance.GetComponentInParent<RigManager>();
        if (rigManager == null)
            return;
        
        if (!NetworkPlayerManager.TryGetPlayer(rigManager, out var player))
            return;
        
        WindBuffetSfxInstances[__instance] = player.PlayerID;
    }

    [HarmonyPatch(nameof(WindBuffetSFX.OnDisable))]
    [HarmonyPostfix]
    public static void OnDisablePostfix(WindBuffetSFX __instance)
    {
        if (__instance == null)
            return;

        WindBuffetSfxInstances.Remove(__instance);
    }

    [HarmonyPatch(nameof(WindBuffetSFX.UpdateBuffet))]
    [HarmonyPrefix]
    public static bool UpdateBuffetPrefix(WindBuffetSFX __instance)
    {
        if (__instance == null)
            return true;

        if (!WindBuffetSfxInstances.TryGetValue(__instance, out var id))
            return true;
        
        if (id.IsSpectating())
            return false;

        return false;
    }
}