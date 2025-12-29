using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Networking.Variable.Encoder.Util;
using MashGamemodeLibrary.Phase.Tags;
using MashGamemodeLibrary.Player.Team;
using MashGamemodeLibrary.Registry.Typed;
using MelonLoader;

namespace MashGamemodeLibrary.Phase;

public static class GamePhaseManager
{
    public static readonly SingletonTypedRegistry<GamePhase> Registry = new();

    private static readonly SyncedVariable<ulong?> WantedPhase = new("GamePhaseManager_WantedPhase", new NullableValueEncoder<ulong>(new ULongEncoder()),
        null);

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
    public static GamePhase? ActivePhase { get; private set; }

    private static bool PhaseHashOnValidate(ulong? id)
    {
        return id == null || Registry.Contains(id.Value);
    }

    public static void Enable<T>() where T : GamePhase
    {
        Executor.RunIfHost(() => WantedPhase.Value = Registry.CreateID<T>());
    }

    public static void Disable()
    {
        Executor.RunIfHost(() => WantedPhase.Value = null);
    }

    private static void PhaseChanged(ulong? id)
    {
        ActivePhase?.Exit();
        ActivePhase = id.HasValue ? Registry.Get(id.Value) : null;

        if (ActivePhase == null) return;

        ActivePhase.Enter();

        EntityTagManager.GetAllExtendingTag<IPhaseChangedTag>().ForEach(tag =>
        {
            try
            {
                tag.OnPhaseChange(ActivePhase);
            }
            catch (Exception exception)
            {
                MelonLogger.Error($"Failed to execute tag change for: {tag.GetType().FullName}", exception);
            }
        });

        TeamManager.OnPhaseChanged(ActivePhase);
    }

    public static void Update(float delta)
    {
        if (ActivePhase == null)
            return;

        CheckHands();

        ActivePhase.Update(delta);

        Executor.RunIfHost(() =>
        {
            var nextPhase = ActivePhase.Try(a => a.GetNextPhase(), PhaseIdentifier.Empty);
            if (nextPhase.TryGetValue(out var value)) WantedPhase.Value = value;
        });
    }

    public static bool IsPhase<T>() where T : GamePhase
    {
        return ActivePhase is T;
    }

    private static void CheckHand(Hand hand)
    {
        var controller = hand._controller;
        var handedness = controller.handedness;

        if (controller._menuTap)
            OnAction(PlayerIDManager.LocalID, PlayerGameActions.Ability, handedness);

        var state = controller.isBelowGripThreshold;
        var lastState = LastGripStateMap.GetValueOrDefault(handedness, false);
        if (state == lastState) return;

        var action = state ? PlayerGameActions.HandClose : PlayerGameActions.HandOpen;
        OnAction(PlayerIDManager.LocalID, action, handedness);
        LastGripStateMap[handedness] = controller.isBelowGripThreshold;
    }

    private static void CheckHands()
    {
        CheckHand(BoneLib.Player.LeftHand);
        CheckHand(BoneLib.Player.RightHand);
    }

    private static void OnAction(PlayerID player, PlayerGameActions action, Handedness handedness = Handedness.BOTH)
    {
        ActivePhase?.OnPlayerAction(player, action, handedness);
    }

    private static PlayerGameActions? GetPhaseAction(PlayerActionType playerAction)
    {
        return playerAction switch
        {
            PlayerActionType.DEATH => PlayerGameActions.Death,
            PlayerActionType.JUMP => PlayerGameActions.Jump,
            PlayerActionType.DYING => PlayerGameActions.Dying,
            _ => null
        };
    }

    // Event Delegators

    private static void OnPlayerAction(PlayerID playerId, PlayerActionType type, PlayerID otherPlayer)
    {
        if (ActivePhase == null)
            return;

        var action = GetPhaseAction(type);
        if (!action.HasValue)
            return;

        OnAction(playerId, action.Value);
    }

    private static void OnPlayerJoined(PlayerID playerId)
    {
        ActivePhase?.OnPlayerJoined(playerId);
    }

    private static void OnPlayerLeft(PlayerID playerId)
    {
        ActivePhase?.OnPlayerLeft(playerId);
    }
}