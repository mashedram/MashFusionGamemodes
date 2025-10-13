using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Scene;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.networking.Validation.Routes;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Patches;
using MashGamemodeLibrary.Vision;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace MashGamemodeLibrary.Spectating;

class ColliderSet
{
    private HashSet<Collider> _colliders;

    public ColliderSet(GameObject root)
    {
        _colliders = root.GetComponentsInChildren<Collider>().ToHashSet();
    }

    public void SetColliding(ColliderSet other, bool colliding)
    {
        foreach (var collider1 in _colliders)
        {
            foreach (var collider2 in other._colliders)
            {
                // Comes from the same source. One invalid all invalid
                if (collider1 == null || collider2 == null) return;
                Physics.IgnoreCollision(collider1, collider2, !colliding);
            }
        }
    }
}

class PlayerColliderCache
{
    private PhysicsRig? _physicsRig;
    private ColliderSet _physicsRigColliders = null!;
    private Dictionary<GameObject, ColliderSet> _itemColliders = new();
    private readonly HashSet<ColliderSet> _ignoredColliders = new();
    private readonly HashSet<PlayerColliderCache> _ignoredPlayers = new();

    public PlayerColliderCache(PhysicsRig physicsRig)
    {
        SetRig(physicsRig);
    }

    private void StartItemColliding(ColliderSet otherColliders)
    {
        if (_physicsRig == null) return;
        if (!_ignoredColliders.Remove(otherColliders)) return;
        _physicsRigColliders.SetColliding(otherColliders, true);
        foreach (var ownItemColliders in _itemColliders.Values)
        {
            ownItemColliders.SetColliding(otherColliders, true);
        }
    }

    private void StopItemColliding(ColliderSet otherColliders)
    {
        if (_physicsRig == null) return;
        if (!_ignoredColliders.Add(otherColliders)) return;
        _physicsRigColliders.SetColliding(otherColliders, false);
        foreach (var ownItemColliders in _itemColliders.Values)
        {
            ownItemColliders.SetColliding(otherColliders, false);
        }
    }

    public void StartColliding(PlayerColliderCache other)
    {
        if (_physicsRig == null) return;
        if (!_ignoredPlayers.Remove(other)) return;
        other._ignoredPlayers.Remove(this);
        _physicsRigColliders.SetColliding(other._physicsRigColliders, true);
        foreach (var otherColliders in other._itemColliders.Values)
        {
            StartItemColliding(otherColliders);
        }
    }

    public void StopColliding(PlayerColliderCache other)
    {
        if (_physicsRig == null) return;
        if (!_ignoredPlayers.Add(other)) return;
        other._ignoredPlayers.Add(this);
        _physicsRigColliders.SetColliding(other._physicsRigColliders, false);
        foreach (var otherColliders in other._itemColliders.Values)
        {
            StopItemColliding(otherColliders);
        }
    }

    public bool IsCollidingWith(PlayerColliderCache other)
    {
        return _ignoredPlayers.Contains(other);
    }

    public void SetRig(PhysicsRig newRig)
    {
        if (_physicsRig == newRig) return;
        _physicsRig = newRig;
        _physicsRigColliders = new ColliderSet(newRig.gameObject);

        foreach (var itemColliders in _itemColliders.Values)
        {
            foreach (var other in _ignoredPlayers)
            {
                other.StartItemColliding(itemColliders);
            }
        }
        
        _itemColliders.Clear();
        
        var inventory = _physicsRig.gameObject.GetComponent<Inventory>();
        foreach (var inventoryBodySlot in inventory.bodySlots)
        {
            var slot = inventoryBodySlot._inventorySlot;
            if (slot == null) continue;
            var weapon = slot._weaponHost;
            if (weapon == null) continue;
            var gameObject = weapon.GetHostGameObject();
            if (gameObject == null) continue;
            AddItem(gameObject);
        }

        var hands = new[] { _physicsRig.leftHand, _physicsRig.rightHand };
        foreach (var hand in hands)
        {
            var attached = hand.m_CurrentAttachedGO;
            if (attached == null) return;
            AddItem(attached);
        }
    }

    public void AddItem(GameObject item)
    {
        if (_itemColliders.ContainsKey(item)) return;
        var set = new ColliderSet(item);
        _itemColliders[item] = set;

        foreach (var other in _ignoredPlayers)
        {
            other.StopItemColliding(set);
        }
    }

    public void RemoveItem(GameObject item)
    {
        if (!_itemColliders.Remove(item, out var colliderSet)) return;

        foreach (var other in _ignoredPlayers)
        {
            other.StartItemColliding(colliderSet);
        }
    }
}

public static class SpectatorManager
{
    private const string SpectatorHideKey = "spectatorhidekey";

    private static GameObject? _visualEffectObject;

    private const string GrabOverwriteKey = "spectating";

    private static bool _isLocalSpectating;

    private static readonly ByteSyncedSet SpectatingPlayerIds =
        new("spectatingPlayerIds", new HostToClientNetworkRoute());

    private static readonly HashSet<byte> HiddenPlayerIds = new();
    private static readonly Dictionary<byte, PlayerColliderCache> PlayerColliders = new();

    static SpectatorManager()
    {
        SpectatingPlayerIds.OnValueAdded += _ => Refresh();
        SpectatingPlayerIds.OnValueRemoved += _ => Refresh();
        
        NetworkPlayer.OnNetworkRigCreated += (player, _) =>
        {
            RefreshPlayer(player);
        };
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

    public static bool IsLocalPlayerSpectating()
    {
        return _isLocalSpectating;
    }

    public static bool IsPlayerSpectating(byte playerId)
    {
        return SpectatingPlayerIds.Contains(playerId);
    }

    private static bool ShouldBeSpectating(NetworkPlayer player)
    {
        var isSpectating = SpectatingPlayerIds.Contains(player.PlayerID);
        var shouldBeHidden = isSpectating && (!_isLocalSpectating || player.PlayerID.IsMe);

        return shouldBeHidden;
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

    private static void GenerateColliderCache(NetworkPlayer player)
    {
        if (!player.HasRig) return;
        var rig = player.RigRefs.RigManager.physicsRig;
        if (PlayerColliders.TryGetValue(player.PlayerID, out var cache))
        {
            cache.SetRig(rig);
        }
        else
        {
            PlayerColliders[player.PlayerID] = new PlayerColliderCache(rig);
        }
    }

    private static void SetColliders(NetworkPlayer player, bool state)
    {
        if (!player.HasRig)
            return;


        var hands = new[] { player.RigRefs.LeftHand, player.RigRefs.RightHand };
        foreach (var hand in hands)
        {
            hand.TryDetach();
            if (state)
            {
                hand.EnableCollider();
            }
            else
            {
                hand.DisableCollider();   
            }
        }
        
        Executor.RunIfMe(player.PlayerID, () =>
        {
            // 6: Fixtures
            // 8: Player
            // 10: Dynamic
            // 12: EnemyColliders
            // 15: Interactable
            // 24: Footbal
            Physics.IgnoreLayerCollision(8, 8, !state);
            Physics.IgnoreLayerCollision(24, 8, !state);
            Physics.IgnoreLayerCollision(24, 24, !state);
            
            Physics.IgnoreLayerCollision(8, 10, !state);
            Physics.IgnoreLayerCollision(24, 10, !state);
            Physics.IgnoreLayerCollision(10, 10, !state);
            
            Physics.IgnoreLayerCollision(8, 12, !state);
            Physics.IgnoreLayerCollision(24, 12, !state);
            
            Physics.IgnoreLayerCollision(8, 6, !state);
            Physics.IgnoreLayerCollision(24, 6, !state);
        });

        if (!PlayerColliders.TryGetValue(player.PlayerID, out var cache))
            return;

        foreach (var otherCollider in PlayerColliders.Values)
        {
            if (otherCollider == cache) continue;
            if (state)
            {
                cache.StartColliding(otherCollider);
            }
            else
            {
                cache.StopColliding(otherCollider);
            }
        }
    }

    private static void Hide(byte smallID)
    {
        if (!HiddenPlayerIds.Add(smallID)) return;
        if (!NetworkPlayerManager.TryGetPlayer(smallID, out var player)) return;
        
        var playerID = player.PlayerID;

        if (player.HasRig)
        {
            player.RigRefs.LeftHand.DetachObject();
            player.RigRefs.RightHand.DetachObject();
        }

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
        PlayerGrabManager.SetOverwrite(GrabOverwriteKey, _ => false);
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

    private static void RefreshPlayer(NetworkPlayer player)
    {
        if (!player.HasRig)
            return;
        
        var shouldBeHidden = ShouldBeSpectating(player);

        GenerateColliderCache(player);

        if (shouldBeHidden)
        {
            Hide(player.PlayerID);
        }
        else
        {
            Show(player.PlayerID);
        }
    }
    
    private static void Refresh()
    {
        _isLocalSpectating = IsLocalPlayerSpectating();
        foreach (var player in NetworkPlayer.Players)
        {
           RefreshPlayer(player);
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

    public static void OnGrab(GrabData grab)
    {
        if (!grab.IsHoldingItem(out var item)) return;

        var player = grab.NetworkPlayer;
        if (!PlayerColliders.TryGetValue(player.PlayerID, out var cache))
            return;

        cache.AddItem(item.GameObject);
    }

    public static void OnDrop(GrabData grab)
    {
        if (!grab.IsHoldingItem(out var item)) return;

        var player = grab.NetworkPlayer;
        if (!PlayerColliders.TryGetValue(player.PlayerID, out var cache)) return;

        cache.RemoveItem(item.GameObject);
    }

    public static void Clear()
    {
        SpectatingPlayerIds.Clear();
    }

    public static void LocalReset()
    {
        HiddenPlayerIds.Clear();
        PlayerColliders.Clear();
    }
}