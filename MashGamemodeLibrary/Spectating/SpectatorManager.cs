using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.Vision;
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
    private const string SpecatorHideKey = "spectatorhidekey";
    
    private static bool _enabled;
    private const string GrabOverwriteKey = "spectating";
    private static readonly RemoteEvent<SpectatorSyncPacket> SyncEvent = new(OnSyncReceived, false);

    private static readonly HashSet<byte> HiddenIds = new();
    private static HashSet<byte> _spectatingPlayerIds = new();

    public static void Enable()
    {
        _enabled = true;
        Clear();
    }

    public static void Disable()
    {
        _enabled = false;
        Clear();
    }

    public static bool IsLocalPlayerSpectating()
    {
        var localPlayer = LocalPlayer.GetNetworkPlayer();
        return localPlayer != null && _spectatingPlayerIds.Contains(localPlayer.PlayerID);
    }
    
    public static bool IsPlayerSpectating(byte playerId)
    {
        return _spectatingPlayerIds.Contains(playerId);
    }

    private static void SetColliders(RigManager rig, bool state)
    {
        var physicsRig = rig.physicsRig;
        if (state)
        {
            physicsRig.leftHand.EnableCollider();
            physicsRig.rightHand.EnableCollider();
        }
        else
        {
            physicsRig.leftHand.DisableCollider();
            physicsRig.rightHand.DisableCollider();
        }
        
        // 8: Player
        // 10: Dynamic
        // 12: EnemyColliders
        Physics.IgnoreLayerCollision(8, 8, !state);
        Physics.IgnoreLayerCollision(8, 10, !state);
        Physics.IgnoreLayerCollision(8, 12, !state);
        
        var bodylog = rig.inventory.specialItems[0]?.GetComponentInChildren<PullCordDevice>(true);
        if (!bodylog)
            return;
        
        bodylog?.gameObject.SetActive(state);
    }

    private static void Hide(PlayerID playerId)
    {
        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player)) return;
        if (!HiddenIds.Add(playerId)) return;

        player.RigRefs.LeftHand.DetachObject();
        player.RigRefs.RightHand.DetachObject();
        
        Executor.RunIfRemote(playerId, () =>
        {
            player.PlayerID.SetHidden(SpecatorHideKey, true);
        });
        
        if (!playerId.IsMe) return;
        SetColliders(player.RigRefs.RigManager, false);
        PlayerGrabManager.SetOverwrite(GrabOverwriteKey, (_, _) => false);
    }

    private static void Show(PlayerID playerId)
    {
        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player)) return;
        if (!HiddenIds.Remove(playerId)) return;

        Executor.RunIfRemote(playerId, () =>
        {
            player.PlayerID.SetHidden(SpecatorHideKey, false);
        });
       
        if (!playerId.IsMe) return;
        SetColliders(player.RigRefs.RigManager, true);
        PlayerGrabManager.SetOverwrite(GrabOverwriteKey, null);
    }
    
    private static void ApplyAll()
    {
        var isLocalSpectating = IsLocalPlayerSpectating();
        foreach (var player in NetworkPlayer.Players)
        {
            var isSpectating = _spectatingPlayerIds.Contains(player.PlayerID);
            var shouldBeHidden = isSpectating && (!isLocalSpectating || player.PlayerID.IsMe);
            
            if (shouldBeHidden && _enabled)
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
        _spectatingPlayerIds = packet.SpectatingPlayerIds.ToHashSet();

        ApplyAll();
    }
    
    private static void SendSync()
    {
        if (!NetworkInfo.IsHost) return;
        var packet = new SpectatorSyncPacket(_spectatingPlayerIds);
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
            _spectatingPlayerIds.Add(playerID);
        }
        else
        {
            _spectatingPlayerIds.Remove(playerID);
        }
        ApplyAll();
    }

    public static void Clear()
    {
        _spectatingPlayerIds.Clear();
        ApplyAll();
    }
}