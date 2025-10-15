using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Phase;

namespace MashGamemodeLibrary.Player.Controller;

public static class PlayerControllerManager
{
    private static Func<NetworkPlayer, PlayerController>? _controllerFactory;
    private static readonly Dictionary<byte, PlayerController> Controllers = new();

    static PlayerControllerManager()
    {
        NetworkPlayer.OnNetworkRigCreated += NetworkPlayerOnOnNetworkRigCreated;

        MultiplayerHooking.OnPlayerAction += OnPlayerAction;
    }
    
    private static T Instantiate<T>(NetworkPlayer player) where T : PlayerController, new()
    {
        var controller = new T();
        controller.Attach(player);
        return controller;
    }
    
    public static void Enable<T>() where T : PlayerController, new()
    {
        Executor.RunIfHost(() =>
        {
            if (_controllerFactory != null)
                return;

            _controllerFactory = Instantiate<T>;

            foreach (var player in NetworkPlayer.Players)
            {
                if (!player.HasRig) 
                    return;

                Controllers[player.PlayerID] = _controllerFactory(player);
            }
        });
    }

    public static void Disable()
    {
        Executor.RunIfHost(() =>
        {
            _controllerFactory = null;
        
            foreach (var controller in Controllers.Values) controller.Detach();
            Controllers.Clear();
        });
    }

    public static T GetController<T>(this PlayerID playerID) where T : PlayerController
    {
        if (Controllers[playerID] is T value) return value;

        throw new Exception(
            "Tried to fetch controller of the wrong time. Did you enable the controller in your gamemode?");
    }

    public static T GetController<T>(this NetworkPlayer player) where T : PlayerController
    {
        return GetController<T>(player.PlayerID);
    }

    public static void OnAll<T>(Action<T> action) where T : PlayerController
    {
        foreach (var networkPlayer in NetworkPlayer.Players)
        {
            if (!Controllers.TryGetValue(networkPlayer.PlayerID, out var controller)) continue;

            if (controller is not T value) continue;
            
            action.Invoke(value);
        }
    }
    
    public static void Update(float delta)
    {
        if (_controllerFactory == null)
            return;

        foreach (var controller in Controllers.Values) controller.OnUpdate(delta);
    }
    
    // Events
    private static void NetworkPlayerOnOnNetworkRigCreated(NetworkPlayer player, RigManager rig)
    {
        if (_controllerFactory == null) return;

        Controllers.TryAdd(player.PlayerID, _controllerFactory(player));
    }

    private static void OnPlayerAction(PlayerID player, PlayerActionType action, PlayerID otherPlayer)
    {
        Executor.RunIfHost(() =>
        {
            if (!Controllers.TryGetValue(player, out var value)) return;
        
            value.OnPlayerAction(action, otherPlayer);
        });
    }
}