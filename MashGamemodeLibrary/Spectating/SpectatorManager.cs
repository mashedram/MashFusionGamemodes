using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.networking;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Spectating;

internal class SpectatorSyncPacket : INetSerializable
{
    public byte[] SpectatingPlayerIds;
    
    public SpectatorSyncPacket(HashSet<byte> spectatingPlayerIds)
    {
        SpectatingPlayerIds = spectatingPlayerIds.ToArray();
    }
    
    public SpectatorSyncPacket()
    {
        SpectatingPlayerIds = Array.Empty<byte>();
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SpectatingPlayerIds);
    }
}

public static class SpectatorManager
{
    private const string GrabOverwriteKey = "spectating";
    private static readonly RemoteEvent<SpectatorSyncPacket> SyncEvent = new(OnSyncReceived, false);

    private static readonly HashSet<byte> HiddenIds = new();
    private static readonly HashSet<byte> SpectatingPlayerIds = new();

    public static bool IsLocalPlayerSpectating()
    {
        var localPlayer = LocalPlayer.GetNetworkPlayer();
        return localPlayer != null && SpectatingPlayerIds.Contains(localPlayer.PlayerID);
    }
    
    public static bool IsSpectating(byte playerId)
    {
        return SpectatingPlayerIds.Contains(playerId);
    }

    private static void Hide(byte playerId)
    {
        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player)) return;
        if (!HiddenIds.Add(playerId)) return;

        player.RigRefs.LeftHand.DetachObject();
        player.RigRefs.LeftHand.DetachObject();
        player.RigRefs.RigManager.gameObject.active = false;
        player.HeadUI.Visible = false;

        player.VoiceSource.VoiceSource.AudioSource.mute = true;
        
        PlayerGrabManager.SetOverwrite(GrabOverwriteKey, false);
    }

    private static void Show(byte playerId)
    {
        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player)) return;
        if (!SpectatingPlayerIds.Remove(playerId)) return;
        
        player.RigRefs.RigManager.gameObject.active = true;
        player.HeadUI.Visible = true;

        // A reset might be needed, there are some cases where the rep seems to freak out really badly, assuming its because the rep is trying to
        // Use velocity to pickup to where its supposed to be, but since its hidden, it cant, so it just freaks out when it comes back.
        player.RigRefs.RigManager.physicsRig.ResetHands(Handedness.BOTH);
        player.RigRefs.RigManager.physicsRig.UnRagdollRig();
        player.RigRefs.RigManager.TeleportToPose(player.RigPose.PelvisPose.position, Vector3.forward, true);

        player.VoiceSource.VoiceSource.AudioSource.mute = false;
        
        PlayerGrabManager.SetOverwrite(GrabOverwriteKey, false);
    }

    private static void ApplyAll()
    {
        var isLocalSpectating = IsLocalPlayerSpectating();
        foreach (var player in NetworkPlayer.Players)
        {
            var isSpectating = SpectatingPlayerIds.Contains(player.PlayerID);
            var shouldBeHidden = isSpectating != isLocalSpectating;
            
            if (shouldBeHidden)
            {
                Hide(player.PlayerID);
            }
            else
            {
                Show(player.PlayerID);
            }
        }
        
        if (!NetworkInfo.IsHost)
            return;
        
        SendSync();
    }
    
    private static void OnSyncReceived(SpectatorSyncPacket packet)
    {
        var allIds = SpectatingPlayerIds.Union(packet.SpectatingPlayerIds);
        
        foreach (var id in allIds)
        {
            var currentlySpectating = SpectatingPlayerIds.Contains(id);
            var shouldBeSpectating = packet.SpectatingPlayerIds.Contains(id);
            if (currentlySpectating != shouldBeSpectating)
            {
                SpectatingPlayerIds.Add(id);
            }
            else
            {
                SpectatingPlayerIds.Remove(id);
            }
        }

        ApplyAll();
    }
    
    private static void SendSync()
    {
        if (!NetworkInfo.IsHost) return;
        var packet = new SpectatorSyncPacket(SpectatingPlayerIds);
        SyncEvent.Call(packet);
    }
    
    public static void SetSpectating(this PlayerID playerID, bool spectating)
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Error("Only the host can set spectating states!");
            return;
        }

        if (spectating)
        {
            SpectatingPlayerIds.Add(playerID);
        }
        else
        {
            SpectatingPlayerIds.Remove(playerID);
        }
        ApplyAll();
    }

    public static void Clear()
    {
        SpectatingPlayerIds.Clear();
        ApplyAll();
    }
}