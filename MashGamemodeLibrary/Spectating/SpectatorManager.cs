using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
    private const string SpectatorHideKey = "spectatorhidekey";

    private static GameObject? _visualEffectObject;
    
    private static bool _enabled;
    private const string GrabOverwriteKey = "spectating";

    private static readonly ByteSyncedSet SpectatingPlayerIds = new("spectatingPlayerIds");
    private static readonly HashSet<byte> HiddenPlayerIds = new();

    static SpectatorManager()
    {
        SpectatingPlayerIds.OnValueAdded += _ => Refresh();
        SpectatingPlayerIds.OnValueRemoved += _ => Refresh();
    }
    
    private static void SetMute(NetworkPlayer player, bool muted)
    {
        var audioSource = player.VoiceSource?.VoiceSource.AudioSource; 
        if (audioSource) 
        { 
            audioSource!.mute = muted;
        }
    }

    private static GameObject GetVisualEffectObject()
    {
        if (_visualEffectObject != null) return _visualEffectObject;

        _visualEffectObject = new GameObject("SpectatorEffect");

        var volume = _visualEffectObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10;
        volume.weight = 1f;

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.sharedProfile = profile;
        
        var colorAdjustments = profile.Add<ColorAdjustments>(true);
        colorAdjustments.saturation.value = -100f;

        return _visualEffectObject;
    }

    private static void ToggleVisualEffect(bool show)
    {
        GetVisualEffectObject().SetActive(show);
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

    private static void DetachAll(RigManager rig)
    {
        foreach (var slot in rig.inventory.bodySlots)
        {
            var receiver = slot.inventorySlotReceiver;
            if (receiver == null) continue;
            receiver.DespawnContents();
        }
    }

    private static void SetPhysicsRigColliders(PhysicsRig rig1, PhysicsRig rig2, bool state)
    {
        var colliders1 = rig1.GetComponentsInChildren<Collider>();
        var colliders2 = rig2.GetComponentsInChildren<Collider>();

        foreach (var collider1 in colliders1)
        {
            foreach (var collider2 in colliders2)
            {
                Physics.IgnoreCollision(collider1, collider2, !state);
            }
        }
    }

    private static void SetColliders(NetworkPlayer player, bool state)
    {
        if (!player.HasRig)
            return;
        
        var rig = player.RigRefs.RigManager;
        
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
        
        Executor.RunIfMe(player.PlayerID, () =>
        {
            // 8: Player
            // 10: Dynamic
            // 12: EnemyColliders
            Physics.IgnoreLayerCollision(8, 8, !state);
            Physics.IgnoreLayerCollision(8, 10, !state);
            Physics.IgnoreLayerCollision(8, 12, !state);
        });
        
        // TODO: Ignore physics of inventory items too
        foreach (var otherplayer in NetworkPlayer.Players)
        {
            if (!otherplayer.HasRig) continue;
            if (otherplayer.PlayerID.Equals(player.PlayerID)) continue;

            SetPhysicsRigColliders(otherplayer.RigRefs.RigManager.physicsRig, player.RigRefs.RigManager.physicsRig, state);
        }
        
        var bodylog = rig.inventory.specialItems[0]?.GetComponentInChildren<PullCordDevice>(true);
        if (!bodylog)
            return;
        
        bodylog?.gameObject.SetActive(state);
    }

    private static void Hide(byte smallID)
    {
        if (!HiddenPlayerIds.Add(smallID)) return;
        if (!NetworkPlayerManager.TryGetPlayer(smallID, out var player)) return;
        
        var playerID = player.PlayerID;

        player.RigRefs.LeftHand.DetachObject();
        player.RigRefs.RightHand.DetachObject();
        
        Executor.RunIfRemote(playerID, () =>
        {
            player.PlayerID.SetHidden(SpectatorHideKey, true);
            SetMute(player, true);
        });
        
        SetColliders(player, false);
        
        if (!playerID.IsMe) return;
        Loadout.Loadout.ClearPlayerLoadout(player.RigRefs);
        ToggleVisualEffect(true);
        DetachAll(player.RigRefs.RigManager);
        DevToolsPatches.CanSpawn = false;
        PlayerGrabManager.SetOverwrite(GrabOverwriteKey, (_, _) => false);
    }

    private static void Show(byte smallID)
    {
        if (!HiddenPlayerIds.Remove(smallID)) return;
        if (!NetworkPlayerManager.TryGetPlayer(smallID, out var player)) return;

        var playerID = player.PlayerID;
        
        Executor.RunIfRemote(playerID, () =>
        {
            player.PlayerID.SetHidden(SpectatorHideKey, false);
            SetMute(player, false);
        });
        
        SetColliders(player, true);
       
        if (!playerID.IsMe) return;
        ToggleVisualEffect(false);
        DevToolsPatches.CanSpawn = true;
        PlayerGrabManager.SetOverwrite(GrabOverwriteKey, null);
    }

    private static void Refresh()
    {
        var isLocalSpectating = IsLocalPlayerSpectating();
        foreach (var player in NetworkPlayer.Players)
        {
            var isSpectating = SpectatingPlayerIds.Contains(player.PlayerID);
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