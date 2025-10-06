using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Interaction.Components;
using MashGamemodeLibrary.Entities.Tagging;
using UnityEngine;

namespace MashGamemodeLibrary.Entities.Interaction;

public static class PlayerGrabManager
{
    private static readonly Dictionary<string, Func<NetworkEntity?, MarrowEntity?, bool>> OverwriteMap = new();
    private static readonly Dictionary<ushort, double> LastGrabbedTime = new();
    private static readonly Dictionary<ushort, double> LastDroppedTime = new();

    static PlayerGrabManager()
    {
        NetworkEntityManager.IDManager.OnEntityUnregistered += entity =>
        {
            LastGrabbedTime.Remove(entity.ID);
            LastDroppedTime.Remove(entity.ID);
        };
    }

    public static bool IsForceDisabled()
    {
        return OverwriteMap.Any(predicate => !predicate.Value.Invoke(null, null));
    }
    
    public static bool IsForceDisabled(NetworkEntity? entity, MarrowEntity? marrowEntity)
    {
        return OverwriteMap.Count != 0 && OverwriteMap.Any(predicate => !predicate.Value.Invoke(entity, marrowEntity));
    }
    
    public static void OnGrab(this MarrowEntity entity, Hand hand)
    {
        if (!IMarrowEntityExtender.Cache.TryGet(entity, out var networkEntity)) return;

        var time = Time.realtimeSinceStartupAsDouble;
        var lastGrabbed = LastGrabbedTime.GetValueOrDefault(networkEntity.ID);
        LastGrabbedTime[networkEntity.ID] = time;
        
        var timeSinceLastGrab = time - lastGrabbed;
        
        var callbacks = networkEntity
            .GetAllExtendingTag<IEntityGrabCallback>();
        
        callbacks
            .Where(e => timeSinceLastGrab > e.GrabCooldown)
            .ForEach(e => e.OnGrab(networkEntity, hand));
    }

    public static void OnDrop(this MarrowEntity entity, Hand hand)
    {
        if (!IMarrowEntityExtender.Cache.TryGet(entity, out var networkEntity)) return;
        
        var time = Time.realtimeSinceStartupAsDouble;
        var lastDropped = LastDroppedTime.GetValueOrDefault(networkEntity.ID);
        LastDroppedTime[networkEntity.ID] = time;
        
        var timeSinceLastDrop = time - lastDropped;
        
        var callbacks = networkEntity
            .GetAllExtendingTag<IEntityDropCallback>();
        
        callbacks.Where(e => timeSinceLastDrop > e.DropCooldown).ForEach(e => e.OnDrop(networkEntity, hand, entity));
    }
    
    public static bool CanGrabEntity(this MarrowEntity entity, NetworkPlayer player)
    {
        // Only apply grab predicates for the local player
        if (!player.PlayerID.IsMe) return true;
        
        if (!IMarrowEntityExtender.Cache.TryGet(entity, out var networkEntity)) return true;
        
        if (IsForceDisabled(networkEntity, entity)) return false;

        var predicates = networkEntity
            .GetAllExtendingTag<IEntityGrabPredicate>();
        
        return predicates.Count == 0 || predicates.Any(predicate => predicate.CanGrab(player, networkEntity, entity));
    }

    public static bool CanGrabEntity(this MarrowEntity entity, Hand hand)
    {
        if (!entity || !hand || !hand.manager)
            return false;
        return !NetworkPlayerManager.TryGetPlayer(hand.manager, out var player) || CanGrabEntity(entity, player);
    }
    
    public static void SetOverwrite(string key, Func<NetworkEntity?, MarrowEntity?, bool>? predicate)
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
}