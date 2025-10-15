using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Player;

namespace MashGamemodeLibrary.Phase;

public abstract class GamePhase
{
    public abstract string Name { get; }
    public bool IsActive { get; private set; }

    // Implementation

    public float ElapsedTime { get; private set; }

    /**
     * The pase will exit immediately if this predicate returns true.
     */
    public abstract PhaseIdentifier GetNextPhase();

    protected virtual void OnPhaseEnter()
    {
    }

    protected virtual void OnPhaseExit()
    {
    }

    protected virtual void OnUpdate()
    {
    }

    // States

    public virtual void OnPlayerAction(PlayerID playerId, PlayerGameActions action, Handedness handedness)
    {
    }

    public virtual void OnPlayerJoined(PlayerID player)
    {
    }

    public virtual void OnPlayerLeft(PlayerID player)
    {
    }

    public void Update(float delta)
    {
        ElapsedTime += delta;
        OnUpdate();
    }

    public void Enter()
    {
        IsActive = true;
        ElapsedTime = 0f;
        OnPhaseEnter();
    }

    public void Exit()
    {
        IsActive = false;
        OnPhaseExit();
    }
}