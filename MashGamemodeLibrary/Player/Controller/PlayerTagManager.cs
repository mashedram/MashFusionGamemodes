using System.Diagnostics.CodeAnalysis;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Entities.Tagging.Player;
using MashGamemodeLibrary.Execution;

namespace MashGamemodeLibrary.Player.Controller;

public static class PlayerTagManager
{
    public static void ClearPlayerTags()
    {
        Executor.RunIfHost(() =>
        {
            foreach (var player in NetworkPlayer.Players)
            {
                player.NetworkEntity.ClearComponents();
            }
        });
    }

    public static bool TryGetComponent<T>(this NetworkPlayer player, [MaybeNullWhen(false)] out T component) where T : class, IComponent
    {
        if (player.NetworkEntity == null)
        {
            component = null;
            return false;
        }

        component = player.NetworkEntity.GetComponent<T>();
        return component != null;
    }

    public static bool HasTag<T>(this NetworkPlayer player) where T : class, IComponent
    {

        return player.NetworkEntity?.GetComponent<T>() != null;
    }

    public static bool HasTag<T>(this NetworkPlayer player, Func<T, bool> predicate) where T : class, IComponent
    {
        if (player.NetworkEntity == null)
            return false;

        var component = player.NetworkEntity.GetComponent<T>();
        return component != null && predicate(component);
    }

    public static void AddTag(this NetworkPlayer player, IComponent component)
    {
        player.NetworkEntity?.AddComponent(component);
    }

    public static bool TryAddTag<T>(this NetworkPlayer player, Func<T> factory) where T : class, IComponent
    {
        if (player.NetworkEntity.GetComponent<T>() != null)
            return false;
        
        player.NetworkEntity.AddComponent(factory());
        return true;
    }

    public static void RemoveTag<T>(this NetworkPlayer player) where T : IComponent
    {
        if (player.NetworkEntity == null)
            return ;

        player.NetworkEntity.RemoveComponent<T>();
    }

    public static void ToggleTag<T>(this NetworkPlayer player, bool state, Func<T> factory) where T : class, IComponent
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