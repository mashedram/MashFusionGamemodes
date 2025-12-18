using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;

namespace MashGamemodeLibrary.Player.Controller;

public static class PlayerTagManager
{
    public static void ClearPlayerTags()
    {
        Executor.RunIfHost(() =>
        {
            foreach (var player in NetworkPlayer.Players)
            {
                EntityTagManager.Remove(player.NetworkEntity.ID);
            }
        });
    }

    public static void OnAll<T>(Action<T> action) where T : PlayerTag
    {
        foreach (var (_, tag) in EntityTagManager.GetAllTags<T>())
        {
            action.Invoke(tag);
        }
    }

    public static bool TryGetTag<T>(this NetworkPlayer player, [MaybeNullWhen(false)] out T tag) where T : PlayerTag
    {
        if (player.NetworkEntity == null)
        {
            tag = null;
            return false;
        }
        
        return player.NetworkEntity.TryGetTag(out tag);
    }

    public static bool HasTag<T>(this NetworkPlayer player) where T : PlayerTag
    {
        if (player.NetworkEntity == null)
            return false;
        
        return player.NetworkEntity.HasTag<T>();
    }
    
    public static bool HasTag<T>(this NetworkPlayer player, Func<T, bool> predicate) where T : PlayerTag
    {
        if (player.NetworkEntity == null)
            return false;
        
        return player.NetworkEntity.TryGetTag<T>(out var tag) && predicate(tag);
    }
    
    public static void AddTag<T>(this NetworkPlayer player, T tag) where T : PlayerTag
    {
        if (player.NetworkEntity == null)
            return;
        
        player.NetworkEntity.AddTag(tag);
    }
    
    public static bool TryAddTag<T>(this NetworkPlayer player, Func<T> factory) where T : PlayerTag
    {
        if (player.NetworkEntity == null)
            return false;
        
        return player.NetworkEntity.TryAddTag(factory);
    }
    
    public static bool RemoveTag<T>(this NetworkPlayer player) where T : PlayerTag
    {
        if (player.NetworkEntity == null)
            return false;
        
        return player.NetworkEntity.RemoveTag<T>();
    }

    public static void ToggleTag<T>(this NetworkPlayer player, bool state, Func<T> factory) where T : PlayerTag
    {
        if (player.NetworkEntity == null)
            return;
        
        if (state)
        {
            player.TryAddTag(factory);
        }
        else
        {
            player.RemoveTag<T>();
        }
    }
}