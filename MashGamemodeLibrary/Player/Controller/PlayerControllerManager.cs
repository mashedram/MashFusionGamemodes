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

public static class PlayerControllerManager
{
    private static readonly List<Func<PlayerTag>> ControllerFactories = new();

    static PlayerControllerManager()
    {
        NetworkPlayer.OnNetworkRigCreated += NetworkPlayerOnOnNetworkRigCreated;
    }
    
    public static void Enable<T>(Func<T> factory) where T : PlayerTag
    {
        Executor.RunIfHost(() =>
        {
            ControllerFactories.Add(factory);

            foreach (var player in NetworkPlayer.Players)
            {
                if (!player.HasRig) 
                    return;
                
                if (player.NetworkEntity.HasTag<T>())
                    continue;

                player.NetworkEntity.AddTag(factory.Invoke());
            }
        });
    }

    public static void Disable()
    {
        Executor.RunIfHost(() =>
        {
            ControllerFactories.Clear();

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
        return player.NetworkEntity.TryGetTag(out tag);
    }
    
    // Events
    private static void NetworkPlayerOnOnNetworkRigCreated(NetworkPlayer player, RigManager rig)
    {
        foreach (var controllerFactory in ControllerFactories)
        {
            player.NetworkEntity.AddTag(controllerFactory.Invoke());
        }
    }
}