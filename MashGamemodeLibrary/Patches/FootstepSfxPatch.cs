using HarmonyLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Audio;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Player.Helpers;
using MashGamemodeLibrary.Player.Spectating;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Patches;

[HarmonyPatch(typeof(FootstepSFX))]
public static class FootstepSfxPatch
{
    private static readonly Dictionary<PlayerID, List<FootstepSFX>> PlayerFootstepSfx = new();
    private static readonly Dictionary<FootstepSFX, PlayerID> FootstepSfxPlayer = new();
    
    static FootstepSfxPatch()
    {
        MultiplayerHooking.OnPlayerLeft += id =>
        {
            if (!PlayerFootstepSfx.Remove(id, out var entries)) 
                return;
            
            foreach (var sfx in entries)
            {
                FootstepSfxPlayer.Remove(sfx);
            }
        };
        
        NetworkPlayer.OnNetworkRigCreated += (player, rig) =>
        {
            var sfxes = rig.GetComponentsInChildren<FootstepSFX>();
            if (sfxes == null)
                return;

            foreach (var sfx in sfxes)
            {
                var playerId = player.PlayerID;
                PlayerFootstepSfx.GetValueOrCreate(playerId).Add(sfx);
                FootstepSfxPlayer[sfx] = playerId;
            }
        };
        
        MultiplayerHooking.OnLoadingBegin += () =>
        {
            PlayerFootstepSfx.Clear();
            FootstepSfxPlayer.Clear();
        };
    }
    
    private static PlayerID? GetPlayerId(FootstepSFX sfx)
    {
        return FootstepSfxPlayer.GetValueOrDefault(sfx);
    }

    [HarmonyPatch(nameof(FootstepSFX.PlayStep))]
    [HarmonyPrefix]
    public static bool PlayStepPrefix(FootstepSFX __instance)
    {
        if (__instance == null)
            return true;
        
        if (!NetworkInfo.HasServer)
            return true;
        
        var playerId = GetPlayerId(__instance);
        if (playerId == null)
            return true;
        
        if (!playerId.IsSpectating())
            return true;
        
        return false;
    }
}