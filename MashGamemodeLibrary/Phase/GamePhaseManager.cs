using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Phase.Tags;
using MashGamemodeLibrary.Registry;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Phase;

public static class GamePhaseManager
{
    public static readonly Registry<GamePhase> Registry = new();
    private static readonly HashSyncedVariable WantedPhase = new("GamePhaseManager_WantedPhase", null,
        CommonNetworkRoutes.HostToClient);

    private static GamePhase? _activePhase;
    public static GamePhase? ActivePhase => _activePhase;

    // Action Logic

    private static readonly Dictionary<Handedness, bool> LastGripStateMap = new();
    
    static GamePhaseManager()
    {
        WantedPhase.OnValidate += PhaseHashOnValidate;
        WantedPhase.OnValueChanged += PhaseChanged;

        // Register things

        MultiplayerHooking.OnPlayerAction += OnPlayerAction;
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
    }

    private static bool PhaseHashOnValidate(ulong? id)
    {
        return id == null || Registry.Contains(id.Value);
    }

    public static void Enable<T>() where T : GamePhase
    {
        Executor.RunIfHost(() => { WantedPhase.Value = Registry.GetID<T>(); });
    }

    public static void Disable()
    {
        _activePhase?.Exit();
        _activePhase = null;
    }

    private static void PhaseChanged(ulong? id)
    {
        _activePhase?.Exit();
        _activePhase = id.HasValue ? Registry.Get(id.Value) : null;
        
        if (_activePhase == null) return;
        
        _activePhase.Enter();

        EntityTagManager.GetAllExtendingTag<IPhaseChangedTag>().ForEach(tag =>
        {
            try
            {
                tag.OnPhaseChange(_activePhase);
            }
            catch (Exception exception)
            {
                MelonLogger.Error($"Failed to execute tag change for: {tag.GetType().FullName}", exception);
            }
        });
    }

    public static void Update(float delta)
    {
        if (_activePhase == null)
            return;

        CheckHands();

        _activePhase.Update(delta);

        Executor.RunIfHost(() =>
        {
            var nextPhase = _activePhase.GetNextPhase();
            if (nextPhase.TryGetValue(out var value)) WantedPhase.Value = value;
        });
    }

    public static bool IsPhase<T>() where T : GamePhase
    {
        return _activePhase is T;
    }

    private static void CheckHand(Hand hand)
    {
        var controller = hand._controller;
        var handedness = controller.handedness;

        if (controller._menuTap) OnAction(PlayerIDManager.LocalID, PhaseAction.Ability, handedness);

        var state = controller.isBelowGripThreshold;
        var lastState = LastGripStateMap.GetValueOrDefault(handedness, false);
        if (state == lastState) return;

        var action = state ? PhaseAction.HandClose : PhaseAction.HandOpen;
        OnAction(PlayerIDManager.LocalID, action, handedness);
        LastGripStateMap[handedness] = controller.isBelowGripThreshold;
    }

    private static void CheckHands()
    {
        CheckHand(BoneLib.Player.LeftHand);
        CheckHand(BoneLib.Player.RightHand);
    }

    private static void OnAction(PlayerID player, PhaseAction action, Handedness handedness = Handedness.BOTH)
    {
        _activePhase?.OnPlayerAction(player, action, handedness);
    }

    private static PhaseAction? GetPhaseAction(PlayerActionType playerAction)
    {
        return playerAction switch
        {
            PlayerActionType.DEATH => PhaseAction.Death,
            PlayerActionType.JUMP => PhaseAction.Jump,
            _ => null
        };
    }

    // Event Delegators

    private static void OnPlayerAction(PlayerID playerId, PlayerActionType type, PlayerID otherPlayer)
    {
        if (_activePhase == null)
            return;

        var action = GetPhaseAction(type);
        if (!action.HasValue)
            return;

        OnAction(playerId, action.Value);
    }

    private static void OnPlayerJoined(PlayerID playerId)
    {
        _activePhase?.OnPlayerJoined(playerId);
    }

    private static void OnPlayerLeft(PlayerID playerId)
    {
        _activePhase?.OnPlayerLeft(playerId);
    }
}