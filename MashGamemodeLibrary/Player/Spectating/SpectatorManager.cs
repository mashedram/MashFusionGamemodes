using System.Diagnostics;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Entities.Interaction;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Patches;
using MashGamemodeLibrary.Player.Stats;
using MashGamemodeLibrary.Vision;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MashGamemodeLibrary.Player.Spectating;

internal class IgnorePropPacket : INetSerializable, IKnownSenderPacket
{
    public byte SenderPlayerID { get; set; }
    public NetworkEntityReference Reference;
    
    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Reference);
    }
}

internal class ColliderSet
{
    private readonly HashSet<Collider> _colliders;
    public IEnumerable<Collider> Colliders => _colliders;

    public ColliderSet(GameObject root)
    {
        _colliders = root.GetComponentsInChildren<Collider>().ToHashSet();
    }

    public ColliderSet(MarrowBody body)
    {
        _colliders = body._colliders.ToHashSet();
    }

    public ColliderSet(MarrowEntity entity)
    {
        _colliders = entity._bodies.SelectMany(body => body._colliders).ToHashSet();
    }

    public void SetColliding(ColliderSet other, bool colliding)
    {
        foreach (var collider1 in _colliders)
        foreach (var collider2 in other._colliders)
        {
            // Comes from the same source. One invalid all invalid
            if (collider1 == null || collider2 == null) return;

            Physics.IgnoreCollision(collider1, collider2, !colliding);
        }
    }
}

internal class PlayerColliderCache
{
    private static readonly HashSet<int> IncludedRemoteLayers = new()
    {
        24,
        8,
        9,
        16
    };
    private static readonly HashSet<int> IncludedLocalLayers = new()
    {
        8,
        9,
        16
    };

    private readonly HashSet<ColliderSet> _ignoredColliders = new();
    private readonly HashSet<PlayerColliderCache> _ignoredPlayers = new();
    private readonly Dictionary<GameObject, ColliderSet> _itemColliders = new();
    private readonly HashSet<ColliderSet> _propColliders = new();
    
    
    private PhysicsRig? _physicsRig;
    private int _layer = 8;
    private Dictionary<GameObject, int> _originalLayer = new();
    private ColliderSet _physicsRigColliders = null!;

    public PlayerColliderCache(PhysicsRig physicsRig)
    {
        SetRig(physicsRig);
    }

    public void ClearPropColliders()
    {
        foreach (var propCollider in _propColliders)
        {
            _physicsRigColliders.SetColliding(propCollider, true);
        }
        _propColliders.Clear();
    }

    public void StopPropColliding(NetworkEntityReference reference)
    {
        if (!reference.TryGetEntity(out var networkEntity))
            return;

        var marrowEntity = networkEntity.GetExtender<IMarrowEntityExtender>();
        if (marrowEntity == null)
            return;

        var colliderSet = new ColliderSet(marrowEntity.MarrowEntity);
        
        _propColliders.Add(colliderSet);
        _physicsRigColliders.SetColliding(colliderSet, false);
    }

    private void StartItemColliding(ColliderSet otherColliders)
    {
        if (_physicsRig == null) return;
        if (!_ignoredColliders.Remove(otherColliders)) return;
        
        _physicsRigColliders.SetColliding(otherColliders, true);
        foreach (var ownItemColliders in _itemColliders.Values) ownItemColliders.SetColliding(otherColliders, true);
    }

    private void StopItemColliding(ColliderSet otherColliders)
    {
        if (_physicsRig == null) return;
        if (!_ignoredColliders.Add(otherColliders)) return;
        
        _physicsRigColliders.SetColliding(otherColliders, false);
        foreach (var ownItemColliders in _itemColliders.Values) ownItemColliders.SetColliding(otherColliders, false);
    }

    public void StartColliding(PlayerColliderCache other)
    {
        if (_physicsRig == null) return;
        if (!_ignoredPlayers.Remove(other)) return;
        
        other._ignoredPlayers.Remove(this);
        _physicsRigColliders.SetColliding(other._physicsRigColliders, true);
        foreach (var otherColliders in other._itemColliders.Values) StartItemColliding(otherColliders);
    }

    public void StopColliding(PlayerColliderCache other)
    {
        if (_physicsRig == null) return;
        if (!_ignoredPlayers.Add(other)) return;
        
        other._ignoredPlayers.Add(this);
        _physicsRigColliders.SetColliding(other._physicsRigColliders, false);
        foreach (var otherColliders in other._itemColliders.Values) StopItemColliding(otherColliders);
    }

    public bool IsCollidingWith(PlayerColliderCache other)
    {
        return _ignoredPlayers.Contains(other);
    }
    
    public void SetIgnoreRaycast(PlayerID target, bool colliding)
    {
        if (_originalLayer.Count > 0)
        {
            foreach (var (go, layer) in _originalLayer)
            {
                go.layer = layer;
            }
            _originalLayer.Clear();
        }
        
        if (colliding)
            return;
        
        foreach (var collider in _physicsRigColliders.Colliders)
        {
            var go = collider.gameObject;
            var cLayer = go.layer;

            if (target.IsMe)
            {
                if (!IncludedLocalLayers.Contains(cLayer))
                    continue;
            }
            else
            {
                if (!IncludedRemoteLayers.Contains(cLayer))
                    continue;
            }

            // 2 Is ignore raycasts
            go.layer = 2;
            _originalLayer[go] = cLayer;
        }
    }

    public void SetRig(PhysicsRig newRig)
    {
        if (_physicsRig == newRig) return;

        _physicsRig = newRig;
        _physicsRigColliders = new ColliderSet(newRig.gameObject);

        foreach (var itemColliders in _itemColliders.Values)
        foreach (var other in _ignoredPlayers)
            other.StartItemColliding(itemColliders);

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

        foreach (var other in _ignoredPlayers) other.StopItemColliding(set);
    }

    public void RemoveItem(GameObject item)
    {
        if (!_itemColliders.Remove(item, out var colliderSet)) return;

        foreach (var other in _ignoredPlayers) other.StartItemColliding(colliderSet);
    }
}

public static class SpectatorManager
{
    private const string SpectatorHideKey = "spectatorhidekey";

    private const string GrabOverwriteKey = "spectating";

    private static GameObject? _visualEffectObject;

    private static bool _isLocalSpectating;

    private static readonly SyncedSet<byte> SpectatingPlayerIds = new("spectatingPlayerIds", new ByteEncoder());

    private static readonly HashSet<byte> HiddenPlayerIds = new();
    
    private static PlayerColliderCache? _localCache;
    private static readonly Dictionary<byte, PlayerColliderCache> PlayerColliders = new();

    private static readonly RemoteEvent<IgnorePropPacket> IgnorePropEvent = new(packet =>
    {
        if (!SpectatingPlayerIds.Contains(packet.SenderPlayerID))
            return;

        if (!PlayerColliders.TryGetValue(packet.SenderPlayerID, out var cache))
            return;

        cache.StopPropColliding(packet.Reference);
    }, CommonNetworkRoutes.AllToAll);

    static SpectatorManager()
    {
        SpectatingPlayerIds.OnValueAdded += _ => Refresh();
        SpectatingPlayerIds.OnValueRemoved += _ => Refresh();

        NetworkPlayer.OnNetworkRigCreated += (player, _) => { RefreshPlayer(player); };
    }

    private static void SetMute(NetworkPlayer player, bool muted)
    {
        var audioSource = player.VoiceSource?.VoiceSource.AudioSource;
        if (audioSource) audioSource!.mute = muted;
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

    public static bool IsPlayerHidden(byte playerId)
    {
        return HiddenPlayerIds.Contains(playerId);
    }

    private static bool ShouldBeSpectating(NetworkPlayer player)
    {
        var isSpectating = SpectatingPlayerIds.Contains(player.PlayerID);
        var shouldBeHidden = isSpectating && (!_isLocalSpectating || player.PlayerID.IsMe);

        return shouldBeHidden;
    }

    private static void GenerateColliderCache(NetworkPlayer player)
    {
        if (!player.HasRig) return;
        
        var rig = player.RigRefs.RigManager.physicsRig;
        if (PlayerColliders.TryGetValue(player.PlayerID, out var cache))
            cache.SetRig(rig);
        else
        {
            var newCache = new PlayerColliderCache(rig);
            if (player.PlayerID.IsMe)
            {
                _localCache = newCache;
            }
            PlayerColliders[player.PlayerID] = newCache;
        }
    }

    private static void SetColliders(NetworkPlayer player, bool state)
    {
        if (!player.HasRig)
            return;


        // var hands = new[] { player.RigRefs.LeftHand, player.RigRefs.RightHand };
        // foreach (var hand in hands)
        // {
        //     hand.TryDetach();
        //     if (state)
        //         hand.EnableCollider();
        //     else
        //         hand.DisableCollider();
        // }

        if (!PlayerColliders.TryGetValue(player.PlayerID, out var cache))
            return;
        
        cache.SetIgnoreRaycast(player.PlayerID, state);

        if (state)
        {
            cache.ClearPropColliders();
        }

        foreach (var otherCache in PlayerColliders.Values.Where(otherCollider => otherCollider != cache))
        {
            if (state)
                cache.StartColliding(otherCache);
            else
                cache.StopColliding(otherCache);
        }
    }

    /// <summary>
    /// Set wether the local player can interact or not
    /// </summary>
    /// <param name="state">True if the player can interact</param>
    private static void SetLocalInteractions(bool state)
    {
        var rig = BoneLib.Player.RigManager;
        if (!state && rig)
        {
            Loadout.Loadout.ClearPlayerLoadout(rig);
        }
        
        LocalControls.DisableInteraction = !state;
        LocalControls.DisableInventory = !state;
        LocalControls.DisableAmmoPouch = !state;
        
        ToggleVisualEffect(!state);
        DevToolsPatches.CanSpawn = state;
        PlayerGrabManager.SetOverwrite(GrabOverwriteKey, state ? null : _ => false);
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

        SetLocalInteractions(false);
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
        
        SetLocalInteractions(true);
    }

    private static void RefreshPlayer(NetworkPlayer player)
    {
        if (!player.HasRig)
            return;

        var shouldBeHidden = ShouldBeSpectating(player);

        GenerateColliderCache(player);

        if (player.PlayerID.IsMe)
        {
            PlayerStatManager.RefreshVitality();
        }

        if (shouldBeHidden)
            Hide(player.PlayerID);
        else
            Show(player.PlayerID);
    }

    private static void Refresh()
    {
        _isLocalSpectating = IsPlayerSpectating(PlayerIDManager.LocalSmallID);
        foreach (var player in NetworkPlayer.Players) RefreshPlayer(player);
    }

    public static void SetSpectating(this PlayerID playerID, bool spectating)
    {
        if (!NetworkInfo.IsHost)
        {
            MelonLogger.Error("Only the host can set spectating states!", new StackTrace());
            return;
        }

        if (spectating)
            SpectatingPlayerIds.Add(playerID);
        else
            SpectatingPlayerIds.Remove(playerID);
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
        if (player == null) return;
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
        
        SetLocalInteractions(true);
        _localCache?.ClearPropColliders();
        _localCache = null;
    }
    
    public static void StartIgnoring(NetworkEntity networkEntity)
    {
        IgnorePropEvent.Call(new IgnorePropPacket
        {
            Reference = new NetworkEntityReference(networkEntity.ID)
        });
    }
}