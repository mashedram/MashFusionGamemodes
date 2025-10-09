using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.networking.Variable.Impl;

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

public class GamePhaseManager
{
    private readonly IntSyncedVariable wantedPhase;
    private bool _enabled;
    private int _activePhaseIndex = 0;
    private GamePhase[] Phases { get; }
    
    public GamePhaseManager(GamePhase[] phases)
    {
        Phases = phases;
        
        wantedPhase = new IntSyncedVariable("GamePhaseManager_WantedPhase", 0);
        wantedPhase.OnValidate += PhaseIndexOnValidate;
        wantedPhase.OnValueChanged += PhaseChanged;
        
        // Register things

        MultiplayerHooking.OnPlayerAction += OnPlayerAction;
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeft;
    }

    private bool PhaseIndexOnValidate(int value)
    {
        return value >= 0 && value < Phases.Length;
    } 
    
    public void Enable(int startPhase = 0)
    {
        if (_enabled) return;
        _enabled = true;
        
        if (wantedPhase.Value == startPhase)
        {
            GetActivePhase().Enter();
        }
        
        Executor.RunIfHost(() =>
        {
            wantedPhase.Value = startPhase;
        });
    }
    
    public void Disable()
    {
        if (!_enabled) return;
        _enabled = false;
        
        GetActivePhase().Exit();
    }
    
    public ref GamePhase GetActivePhase()
    {
        var index = Math.Clamp(_activePhaseIndex, 0, Phases.Length - 1);
        return ref Phases[index];
    }
    
    private void PhaseChanged(int newPhaseIndex)
    {
        if (!_enabled)
            return;
        
        GetActivePhase().Exit();
        _activePhaseIndex = newPhaseIndex;
        GetActivePhase().Enter();
    }
    
    public void MoveToNextPhase()
    {
        var nextPhaseIndex = wantedPhase.Value;
        do
        {
            nextPhaseIndex++;
            
            if (nextPhaseIndex >= Phases.Length) return;

            var nextPhase = Phases[nextPhaseIndex];
            if (nextPhase.CanEnterPhase()) break;
        } while (nextPhaseIndex < Phases.Length && nextPhaseIndex >= 0);
        
        wantedPhase.Value = nextPhaseIndex;
    }

    public void Update(float delta)
    {
        if (!_enabled)
            return;
        
        var activePhase = GetActivePhase();
        activePhase.Update(delta);
        
        Executor.RunIfHost(() =>
        {
            if (activePhase.ShouldMoveToNextPhase())
                MoveToNextPhase();
        });
    }

    public bool IsPhase<T>() where T : GamePhase
    {
        return Phases[_activePhaseIndex] is T;
    }
    
    // Event Delegators

    private void OnPlayerAction(PlayerID playerId, PlayerActionType type, PlayerID otherPlayer)
    {
        if (!_enabled)
            return;
        GetActivePhase().OnPlayerAction(playerId, type, otherPlayer);
    }

    private void OnPlayerJoined(PlayerID playerId)
    {
        if (!_enabled)
            return;
        GetActivePhase().OnPlayerJoined(playerId);
    }
    
    private void OnPlayerLeft(PlayerID playerId)
    {
        if (!_enabled)
            return;
        GetActivePhase().OnPlayerLeft(playerId);
    }
}