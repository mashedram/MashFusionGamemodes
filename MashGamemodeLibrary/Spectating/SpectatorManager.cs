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
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Patches;
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

    private static readonly ByteSyncedSet SpectatingPlayerIds = new("spectatingPlayerIds", CatchupMoment.LevelLoad);

    static SpectatorManager()
    {
        SpectatingPlayerIds.OnValueAdded += Hide;
        SpectatingPlayerIds.OnValueRemoved += Show;
    }

    public static void Enable()
    {
        _enabled = true;
        
        Executor.RunIfHost(Clear);
    }

    public static void Disable()
    {
        _enabled = false;
        
        Executor.RunIfHost(Clear);
    }

    public static bool IsLocalPlayerSpectating()
    {
        var localPlayer = LocalPlayer.GetNetworkPlayer();
        return localPlayer != null && SpectatingPlayerIds.Contains(localPlayer.PlayerID);
    }
    
    public static bool IsPlayerSpectating(byte playerId)
    {
        return SpectatingPlayerIds.Contains(playerId);
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
        // TODO: Add footbal layer to dynamic and enemy colliders too
        
        var bodylog = rig.inventory.specialItems[0]?.GetComponentInChildren<PullCordDevice>(true);
        if (!bodylog)
            return;
        
        bodylog?.gameObject.SetActive(state);
    }

    private static void Hide(byte smallID)
    {
        if (!NetworkPlayerManager.TryGetPlayer(smallID, out var player)) return;
        
        var playerID = player.PlayerID;

        player.RigRefs.LeftHand.DetachObject();
        player.RigRefs.RightHand.DetachObject();
        
        Executor.RunIfRemote(playerID, () =>
        {
            player.PlayerID.SetHidden(SpecatorHideKey, true);
        });
        
        if (!playerID.IsMe) return;
        DevToolsPatches.CanSpawn = false;
        SetColliders(player.RigRefs.RigManager, false);
        PlayerGrabManager.SetOverwrite(GrabOverwriteKey, (_, _) => false);
    }

    private static void Show(byte smallID)
    {
        if (!NetworkPlayerManager.TryGetPlayer(smallID, out var player)) return;

        var playerID = player.PlayerID;
        
        Executor.RunIfRemote(playerID, () =>
        {
            player.PlayerID.SetHidden(SpecatorHideKey, false);
        });
       
        if (!playerID.IsMe) return;
        DevToolsPatches.CanSpawn = true;
        SetColliders(player.RigRefs.RigManager, true);
        PlayerGrabManager.SetOverwrite(GrabOverwriteKey, null);
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
    }

    public static void Clear()
    {
        SpectatingPlayerIds.Clear();
    }
}