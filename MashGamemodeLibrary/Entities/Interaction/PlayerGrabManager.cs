using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using MashGamemodeLibrary.Entities.Interaction.Components;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Spectating;
using MashGamemodeLibrary.Vision;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Entities.Interaction;

public class HeldItem
{
    public readonly GameObject GameObject;
    public readonly Grip Grip;
    public readonly MarrowEntity? MarrowEntity;
    public readonly NetworkEntity? NetworkEntity;

    public HeldItem(IGrippable host)
    {
        Grip = host.GetGrip();
        GameObject = host.GetHostGameObject() ?? Grip.gameObject;
        MarrowEntity = Grip?._marrowEntity;
        if (Grip == null) return;
        GripExtender.Cache.TryGet(Grip, out NetworkEntity);
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

        if (!hand.HasAttachedObject()) return;
        if (!hand.AttachedReceiver.HasHost) return;
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
    private static readonly Dictionary<ushort, double> LastGrabbedTime = new();
    private static readonly Dictionary<ushort, double> LastDroppedTime = new();

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
        SpectatorManager.OnGrab(grab);

        // Callbacks to external systems

        if (IsForceDisabled(grab)) return;

        var id = networkEntity.ID;

        var time = Time.realtimeSinceStartupAsDouble;
        var lastGrabbed = LastGrabbedTime.GetValueOrDefault(id);
        LastGrabbedTime[id] = time;

        var timeSinceLastGrab = time - lastGrabbed;

        var callbacks = networkEntity
            .GetAllExtendingTag<IEntityGrabCallback>();

        callbacks
            .Where(e => timeSinceLastGrab > e.GrabCooldown)
            .ForEach(e => e.OnGrab(grab));
    }

    public static void OnDrop(GrabData grab)
    {
        if (!grab.IsHoldingItem(out var item)) return;
        if (!item.IsNetworked(out var networkEntity)) return;

        // Callbacks to internal systems

        PlayerHider.OnDrop(grab);
        SpectatorManager.OnDrop(grab);

        // Callback to external systems

        var id = networkEntity.ID;

        var time = Time.realtimeSinceStartupAsDouble;
        var lastDropped = LastDroppedTime.GetValueOrDefault(id);
        LastDroppedTime[id] = time;

        var timeSinceLastDrop = time - lastDropped;

        var callbacks = networkEntity
            .GetAllExtendingTag<IEntityDropCallback>();

        callbacks.Where(e => timeSinceLastDrop > e.DropCooldown).ForEach(e => e.OnDrop(grab));
    }

    public static bool CanGrabEntity(GrabData grab)
    {
        // Only apply grab predicates for the local player
        if (grab.NetworkPlayer == null) return true;
        if (!grab.NetworkPlayer.PlayerID.IsMe) return true;
        if (!grab.IsHoldingItem(out var item)) return true;
        if (!item.IsNetworked(out var networkEntity)) return true;

        if (IsForceDisabled(grab)) return false;

        var grabbedRig = item.GameObject.GetComponentInParent<RigManager>();
        if (grabbedRig && NetworkPlayerManager.TryGetPlayer(grabbedRig, out var networkPlayer) &&
            SpectatorManager.IsPlayerSpectating(networkPlayer.PlayerID))
            return false;

        var predicates = networkEntity
            .GetAllExtendingTag<IEntityGrabPredicate>();

        return predicates.Count == 0 || predicates.Any(predicate => predicate.CanGrab(grab));
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

    public static void ClearOverwrites()
    {
        OverwriteMap.Clear();
    }

    public static void Remove(ushort entityID)
    {
        LastGrabbedTime.Remove(entityID);
        LastDroppedTime.Remove(entityID);
    }
}