using System.Diagnostics.CodeAnalysis;
using LabFusion.Entities;
using MashGamemodeLibrary.Entities.ECS;
using MashGamemodeLibrary.Entities.ECS.Declerations;
using MashGamemodeLibrary.Execution;

namespace MashGamemodeLibrary.Player.Helpers;

public static class PlayerComponentExtender
{
    public static void ClearPlayerComponents()
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

    public static bool HasComponent<T>(this NetworkPlayer player) where T : class, IComponent
    {

        return player.NetworkEntity?.GetComponent<T>() != null;
    }

    public static bool HasComponent<T>(this NetworkPlayer player, Func<T, bool> predicate) where T : class, IComponent
    {
        if (player.NetworkEntity == null)
            return false;

        var component = player.NetworkEntity.GetComponent<T>();
        return component != null && predicate(component);
    }

    public static void AddComponent(this NetworkPlayer player, IComponent component)
    {
        player.NetworkEntity?.AddComponent(component);
    }
    
    public static void AddComponents(this NetworkPlayer player, params IComponent[] components)
    {
        if (player.NetworkEntity == null)
            return;

        foreach (var component in components)
        {
            player.NetworkEntity.AddComponent(component);
        }
    }
    
    public static bool TryAddComponent<T>(this NetworkPlayer player, Func<T> factory) where T : class, IComponent
    {
        if (player.NetworkEntity.GetComponent<T>() != null)
            return false;

        player.NetworkEntity.AddComponent(factory());
        return true;
    }

    public static void RemoveComponent<T>(this NetworkPlayer player) where T : class, IComponent
    {
        player.NetworkEntity?.RemoveComponent<T>();
    }

    public static void ToggleComponent<T>(this NetworkPlayer player, bool state, Func<T> factory) where T : class, IComponent
    {
        if (player.NetworkEntity == null)
            return;

        if (state)
        {
            player.TryAddComponent(factory);
        }
        else
        {
            player.RemoveComponent<T>();
        }
    }
}