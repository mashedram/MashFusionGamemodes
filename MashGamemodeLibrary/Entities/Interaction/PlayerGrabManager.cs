using System.Diagnostics.CodeAnalysis;
using Il2CppInterop.Runtime;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Interaction.Components;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Base;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Player.Collision;
using MashGamemodeLibrary.Player.Spectating;
using MashGamemodeLibrary.Player.Visibility;
using MashGamemodeLibrary.Vision;
using MelonLoader;
using UnityEngine;
using Type = Il2CppSystem.Type;

namespace MashGamemodeLibrary.Entities.Interaction;

public record GripWithHand(NetworkEntity NetworkEntity, Grip Grip, Hand Hand);

public class HeldItem
{
    private static readonly Type InteractableHostType = Il2CppType.Of<InteractableHost>();

    public readonly GameObject? GameObject;
    public readonly Grip? Grip;
    public readonly InteractableHost? InteractableHost;
    public readonly MarrowEntity? MarrowEntity;
    public readonly NetworkEntity? NetworkEntity;

    public HeldItem(IGrippable host)
    {
        Grip = host.GetGrip();
        GameObject = host.GetHostGameObject() ?? Grip?.gameObject;
        MarrowEntity = Grip?._marrowEntity;

        if (Grip == null) return;
        GripExtender.Cache.TryGet(Grip, out NetworkEntity);

        if (MarrowEntity == null) return;
        var behaviors = MarrowEntity._behaviours.ToArray();
        foreach (var behavior in behaviors)
        {
            InteractableHost = behavior.TryCast<InteractableHost>();
            if (InteractableHost != null)
                break;
        }
    }

    public HeldItem(Grip grip)
    {
        Grip = grip;
        MarrowEntity = Grip._marrowEntity;
        GameObject = MarrowEntity?.gameObject ?? Grip.gameObject;
        if (Grip == null) return;
        GripExtender.Cache.TryGet(Grip, out NetworkEntity);
    }

    public bool IsNetworked()
    {
        return NetworkEntity != null;
    }

    public bool IsNetworked([MaybeNullWhen(false)] out NetworkEntity entity)
    {
        entity = NetworkEntity;
        return NetworkEntity != null;
    }
}

public class GrabData
{
    public readonly Hand Hand;
    public readonly HeldItem? HeldItem;
    public readonly NetworkPlayer? NetworkPlayer;

    public GrabData(Hand hand)
    {
        Hand = hand;
        GetNetworkEntity(out NetworkPlayer);

        if (hand.AttachedReceiver == null) return;
        if (hand.AttachedReceiver.Host == null) return;
        HeldItem = new HeldItem(hand.AttachedReceiver.Host);
    }

    public GrabData(Hand hand, GameObject gameObject)
    {
        Hand = hand;
        GetNetworkEntity(out NetworkPlayer);

        if (!gameObject.TryGetComponent<Grip>(out var grip)) return;
        HeldItem = new HeldItem(grip);
    }

    public GrabData(Hand hand, Grip grip)
    {
        Hand = hand;
        GetNetworkEntity(out NetworkPlayer);

        if (!grip.HasHost) return;
        HeldItem = new HeldItem(grip.Host);
    }

    public GrabData(Hand hand, InventorySlotReceiver slot)
    {
        Hand = hand;
        GetNetworkEntity(out NetworkPlayer);

        if (slot._weaponHost == null) return;
        HeldItem = new HeldItem(slot._weaponHost);
    }

    private void GetNetworkEntity(out NetworkPlayer? player)
    {
        player = null;
        if (!NetworkInfo.HasServer) return;
        if (Hand.manager == null) return;
        if (NetworkPlayerManager.TryGetPlayer(Hand.manager, out player)) return;
        MelonLogger.Error("Failed to get player from hand manager");
    }

    public bool IsHoldingItem()
    {
        return HeldItem != null;
    }

    public bool IsHoldingItem([MaybeNullWhen(false)] out HeldItem item)
    {
        if (HeldItem == null)
        {
            item = null;
            return false;
        }

        item = HeldItem;
        return true;
    }
}

public static class PlayerGrabManager
{
    private static readonly Dictionary<string, Func<GrabData, bool>> OverwriteMap = new();

    static PlayerGrabManager()
    {
        MultiplayerHooking.OnPlayerAction += OnPlayerAction;
    }

    public static bool IsForceDisabled(GrabData grab)
    {
        return OverwriteMap.Count != 0 && OverwriteMap.Any(predicate => !predicate.Value.Invoke(grab));
    }

    public static void OnGrab(GrabData grab)
    {
        if (!grab.IsHoldingItem(out var heldItem)) return;
        if (!heldItem.IsNetworked(out var networkEntity)) return;

        // Callbacks to internal systems

        PlayerHider.OnGrab(grab);
        PlayerColliderManager.OnGrab(grab);

        // Callbacks to external systems

        var interactableHost = heldItem.InteractableHost;
        if (interactableHost == null)
            return;
        if (interactableHost.HandCount() > 1)
            return;

        var callbacks = networkEntity
            .GetAllExtendingTag<IEntityGrabCallback>();

        callbacks
            .ForEach(e => e.Try(innerE => innerE.OnGrab(grab)));
    }

    public static void OnDrop(GrabData grab)
    {
        if (!grab.IsHoldingItem(out var heldItem)) return;
        if (!heldItem.IsNetworked(out var networkEntity)) return;

        // Callbacks to internal systems

        PlayerHider.OnDrop(grab);
        PlayerColliderManager.OnDrop(grab);

        // Callback to external systems

        var interactableHost = heldItem.InteractableHost;
        if (interactableHost == null)
            return;
        // We will still find a hand. Due to the prefix in GrabPatches
        // the hand count will be 1
        if (interactableHost.HandCount() > 1)
            return;

        var callbacks = networkEntity
            .GetAllExtendingTag<IEntityDropCallback>();

        callbacks
            .ForEach(e => e.OnDrop(grab));
    }

    public static bool CanGrabEntity(GrabData grab)
    {
        // Only apply grab predicates for the local player
        if (grab.NetworkPlayer == null) return true;
        if (!grab.NetworkPlayer.PlayerID.IsMe) return true;
        if (!grab.IsHoldingItem(out var item)) return true;
        if (item.GameObject == null) return true;
        if (!item.IsNetworked(out var networkEntity)) return true;

        if (IsForceDisabled(grab)) return false;

        var grabbedRig = item.GameObject.GetComponentInParent<RigManager>();
        if (grabbedRig && NetworkPlayerManager.TryGetPlayer(grabbedRig, out var networkPlayer) &&
            SpectatorManager.IsSpectating(networkPlayer.PlayerID))
            return false;

        var predicates = networkEntity
            .GetAllExtendingTag<IEntityGrabPredicate>();

        return predicates.Count == 0 || predicates.Any(predicate => predicate.Try(p=> p.CanGrab(grab), false));
    }

    public static bool IsHoldingTag<T>(Hand hand) where T : IEntityTag
    {
        if (!hand.HasAttachedObject()) return false;

        var attached = hand.AttachedReceiver;
        var rb = attached?.Host?.Rb;
        if (rb == null) return false;
        if (!MarrowBody.Cache.TryGet(rb.gameObject, out var body)) return false;
        if (!MarrowBodyExtender.Cache.TryGet(body, out var entity)) return false;

        return entity.HasTag<T>();
    }

    public static bool IsHoldingTag<T>(NetworkPlayer player) where T : IEntityTag
    {
        if (!player.HasRig) return false;

        return IsHoldingTag<T>(player.RigRefs.RightHand) || IsHoldingTag<T>(player.RigRefs.LeftHand);
    }

    public static void SetOverwrite(string key, Func<GrabData, bool>? predicate)
    {
        if (predicate == null)
        {
            OverwriteMap.Remove(key);
            return;
        }

        OverwriteMap[key] = predicate;
    }

    public static void Reset()
    {
        OverwriteMap.Clear();
    }

    public static void DetachIfTag(Hand hand)
    {
        if (!hand.HasAttachedObject()) return;

        var attached = hand.AttachedReceiver;
        var rb = attached?.Host?.Rb;
        if (rb == null) return;

        if (!MarrowBody.Cache.TryGet(rb.gameObject, out var body)) return;
        if (!MarrowBodyExtender.Cache.TryGet(body, out var entity)) return;

        if (!entity.HasTagExtending<IEntityDropCallback>())
            return;

        hand.TryDetach();
    }

    public static IEnumerable<GripWithHand> GetLocalHandsHoldingTag<T>() where T : IEntityTag
    {
        var localPlayer = LocalPlayer.GetNetworkPlayer();
        if (localPlayer is not { HasRig: true })
            return Array.Empty<GripWithHand>();
        
        var hands = new[]
        {
            localPlayer.RigRefs.LeftHand,
            localPlayer.RigRefs.RightHand
        };
        
        return hands
            .Select(hand =>
            {
                if (!hand.HasAttachedObject())
                    return null;

                var attachedGrip = hand.AttachedReceiver?.Host?.GetGrip();
                if (attachedGrip == null)
                    return null;

                if (!GripExtender.Cache.TryGet(attachedGrip, out var gripEntity))
                    return null;

                return new GripWithHand(gripEntity, attachedGrip, hand);
            })
            .OfType<GripWithHand>()
            .Where(hand => hand.NetworkEntity.HasTag<T>());
    }

    public static IEnumerable<Hand> GetLocalHandsHoldingItem(NetworkEntity networkEntity)
    {
        var localPlayer = LocalPlayer.GetNetworkPlayer();
        if (localPlayer is not { HasRig: true })
            return Array.Empty<Hand>();

        var hands = new[]
        {
            localPlayer.RigRefs.LeftHand,
            localPlayer.RigRefs.RightHand
        };

        return hands.Where(hand =>
        {
            if (!hand.HasAttachedObject())
                return false;

            var attachedGrip = hand.AttachedReceiver?.Host?.GetGrip();
            if (attachedGrip == null)
                return false;

            if (!GripExtender.Cache.TryGet(attachedGrip, out var gripEntity))
                return false;

            return gripEntity.ID == networkEntity.ID;
        });
    }

    public static IEnumerable<Hand> GetHandsHoldingItem(NetworkEntity networkEntity)
    {
        var gripExtension = networkEntity.GetExtender<GripExtender>();
        if (gripExtension == null)
            return Array.Empty<Hand>();

        return gripExtension.Components
            .SelectMany(grip => grip.attachedHands._items);
    }

    public static IEnumerable<NetworkPlayer> GetPlayersHoldingItem(NetworkEntity networkEntity)
    {
        return GetHandsHoldingItem(networkEntity)
            .Select(hand => NetworkPlayer.RigCache.Get(hand.manager))
            .Where(player => player != null)
            .DistinctBy(player => player.PlayerID);
    }

    // Events

    private static void OnPlayerAction(PlayerID playerId, PlayerActionType type, PlayerID otherPlayer)
    {
        if (type != PlayerActionType.DYING)
            return;

        // We need to call drop shenanigans on player held items when the player dies
        if (!NetworkPlayerManager.TryGetPlayer(playerId, out var player))
            return;

        if (!player.HasRig)
            return;

        DetachIfTag(player.RigRefs.LeftHand);
        DetachIfTag(player.RigRefs.RightHand);
    }
}