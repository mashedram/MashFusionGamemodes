using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Player;
using UnityEngine;

namespace MashGamemodeLibrary.Spectating;

public static class PlayerIdExtension
{
    private static readonly List<RigManager> HiddenManagers = new List<RigManager>();
    private static readonly List<PlayerID> HiddenIds = new List<PlayerID>();
    private static readonly Dictionary<PlayerID, RigHeadUI> HeadIcons = new();
    
    public static bool IsSpectating(this PlayerID playerId)
    {
        return HiddenIds.Contains(playerId);
    }

    public static void Hide(this PlayerID playerId)
    {
        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player)) return;
        if (HiddenManagers.Contains(player.RigRefs.RigManager)) return;

        HiddenManagers.Add(player.RigRefs.RigManager);
        player.RigRefs.LeftHand.DetachObject();
        player.RigRefs.LeftHand.DetachObject();
        player.RigRefs.RigManager.gameObject.active = false;
        player.HeadUI.Visible = false;
        HiddenIds.Add(playerId);

        player.VoiceSource.VoiceSource.AudioSource.mute = true;
    }

    public static void Show(this PlayerID playerId)
    {
        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player)) return;

        if (!HiddenManagers.Contains(player.RigRefs.RigManager)) return;

        HiddenManagers.Remove(player.RigRefs.RigManager);
        HiddenIds.Remove(playerId);
        player.RigRefs.RigManager.gameObject.active = true;
        player.HeadUI.Visible = true;

        // A reset might be needed, there are some cases where the rep seems to freak out really badly, assuming its because the rep is trying to
        // Use velocity to pickup to where its supposed to be, but since its hidden, it cant, so it just freaks out when it comes back.
        player.RigRefs.RigManager.physicsRig.ResetHands(Handedness.BOTH);
        player.RigRefs.RigManager.physicsRig.UnRagdollRig();
        player.RigRefs.RigManager.TeleportToPose(player.RigPose.PelvisPose.position, Vector3.forward, true);

        player.VoiceSource.VoiceSource.AudioSource.mute = false;
    }
}