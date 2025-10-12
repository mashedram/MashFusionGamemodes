using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Entities.Tagging;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Phase.Tags;

namespace MashGamemodeLibrary.Phase;

class PhaseChangePacket : INetSerializable
{
    public int Index;

    public int? GetSize()
    {
        return sizeof(int);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Index);
    }
}

public static class GamePhaseManager
{
    private static readonly IntSyncedVariable WantedPhase = new("GamePhaseManager_WantedPhase", 0);
    private static bool _enabled;
    private static int _activePhaseIndex;
    private static GamePhase[] _phases = Array.Empty<GamePhase>();
    
    static GamePhaseManager()
    {
        WantedPhase.OnValidate += PhaseIndexOnValidate;
        WantedPhase.OnValueChanged += PhaseChanged;
        
        // Register things

        MultiplayerHooking.OnPlayerAction += OnPlayerAction;
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
    }

    private static bool PhaseIndexOnValidate(int value)
    {
        return value >= 0 && value < _phases.Length;
    } 
    
    public static void Enable(GamePhase[] phases, int startPhase = 0)
    {
        if (_enabled)
        {
            Disable();
        }
        _enabled = true;
        
        _phases = phases;
        
        if (WantedPhase.Value == startPhase)
        {
            GetActivePhase().Enter();
        }
        
        Executor.RunIfHost(() =>
        {
            WantedPhase.Value = startPhase;
        });
    }
    
    public static void Disable()
    {
        if (!_enabled) return;
        _enabled = false;
        
        GetActivePhase().Exit();
    }
    
    public static ref GamePhase GetActivePhase()
    {
        var index = Math.Clamp(_activePhaseIndex, 0, _phases.Length - 1);
        return ref _phases[index];
    }
    
    private static void PhaseChanged(int newPhaseIndex)
    {
        if (!_enabled)
            return;
        
        GetActivePhase().Exit();
        _activePhaseIndex = newPhaseIndex;
        GetActivePhase().Enter();
        
        EntityTagManager.GetAllExtendingTag<IPhaseChangedTag>().ForEach(tag => tag.OnPhaseChange(GetActivePhase()));
    }
    
    public static void MoveToNextPhase()
    {
        var nextPhaseIndex = WantedPhase.Value;
        do
        {
            nextPhaseIndex++;
            
            if (nextPhaseIndex >= _phases.Length) return;

            var nextPhase = _phases[nextPhaseIndex];
            if (nextPhase.CanEnterPhase()) break;
        } while (nextPhaseIndex < _phases.Length && nextPhaseIndex >= 0);
        
        WantedPhase.Value = nextPhaseIndex;
    }

    public static void Update(float delta)
    {
        if (!_enabled)
            return;
        
        CheckHands();
        
        var activePhase = GetActivePhase();
        activePhase.Update(delta);
        
        Executor.RunIfHost(() =>
        {
            if (activePhase.ShouldMoveToNextPhase())
                MoveToNextPhase();
        });
    }

    public static bool IsPhase<T>() where T : GamePhase
    {
        return _phases[_activePhaseIndex] is T;
    }
    
    // Action Logic

    private static readonly Dictionary<Handedness, bool> LastGripStateMap = new();
    
    private static void CheckHand(Hand hand)
    {
        var controller = hand._controller;
        var handedness = controller.handedness;
        
        if (controller._menuTap)
        {
            OnAction(PlayerIDManager.LocalID, PhaseAction.Ability, handedness);
        }

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
        GetActivePhase().OnPlayerAction(player, action, handedness);
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
        if (!_enabled)
            return;

        var action = GetPhaseAction(type);
        if (!action.HasValue)
            return;
        
        OnAction(playerId, action.Value);
    }

    private static void OnPlayerJoined(PlayerID playerId)
    {
        if (!_enabled)
            return;
        GetActivePhase().OnPlayerJoined(playerId);
    }
    
    private static void OnPlayerLeft(PlayerID playerId)
    {
        if (!_enabled)
            return;
        GetActivePhase().OnPlayerLeft(playerId);
    }
}