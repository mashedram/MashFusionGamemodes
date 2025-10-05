using LabFusion.Network;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking;

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
    private readonly RemoteEvent<PhaseChangePacket> _phaseChangeEvent;
    private int WantedPhaseIndex { get; set; } = 0;
    private int _activePhaseIndex = 0;
    private GamePhase[] Phases { get; }
    
    public GamePhaseManager(GamePhase[] phases)
    {
        Phases = phases;
        
        _phaseChangeEvent = new RemoteEvent<PhaseChangePacket>(OnPhaseChange, true);
    }
    
    public ref GamePhase GetActivePhase()
    {
        var index = Math.Clamp(_activePhaseIndex, 0, Phases.Length - 1);
        return ref Phases[index];
    }

    private void OnPhaseChange(PhaseChangePacket packet)
    {
        WantedPhaseIndex = packet.Index;
        
        if (WantedPhaseIndex < 0 || WantedPhaseIndex >= Phases.Length) return;
        
        GetActivePhase().Exit();
        _activePhaseIndex = WantedPhaseIndex;
        GetActivePhase().Enter();
    }
    
    private void SetPhase(int index)
    {
       _phaseChangeEvent.Call(new PhaseChangePacket()
       {
           Index = index
       });
    }
    
    public void MoveToNextPhase()
    {
        var nextPhaseIndex = WantedPhaseIndex;
        do
        {
            nextPhaseIndex++;
            
            if (nextPhaseIndex >= Phases.Length) return;

            var nextPhase = Phases[nextPhaseIndex];
            if (nextPhase.CanEnterPhase()) break;
        } while (nextPhaseIndex < Phases.Length && nextPhaseIndex >= 0);
        
        SetPhase(nextPhaseIndex);
    }
    
    public void ResetPhases()
    {
        Executor.RunIfHost(() =>
        {
            SetPhase(0);
        });
    }

    public void Update(float delta)
    {
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
}