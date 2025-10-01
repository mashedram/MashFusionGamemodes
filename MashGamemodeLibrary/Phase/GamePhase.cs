namespace MashGamemodeLibrary.Phase;

public abstract class GamePhase
{
    public abstract string Name { get; }
    public abstract float Duration { get; }

    /**
     * The phase can only be entered if this predicate returns true.
     */
    protected virtual bool PhaseEnterPredicate()
    {
        return true;
    }
    
    /**
     * The pase will exit immediately if this predicate returns true.
     */
    protected virtual bool ShouldPhaseExit()
    {
        return false;
    }
    
    protected virtual void OnPhaseEnter()
    {
        
    }
    
    protected virtual void OnPhaseExit()
    {
        
    }
    
    protected virtual void OnUpdate()
    {
        
    }
    
    // Implementation
    
    public float ElapsedTime { get; private set; }
    public float RemainingTime => Duration - ElapsedTime;

    public void Update(float delta)
    {
        ElapsedTime += delta;
        OnUpdate();
    }

    public void Enter()
    {
        ElapsedTime = 0f;
        OnPhaseEnter();
    }
    
    public void Exit()
    {
        OnPhaseExit();
    }
    
    public bool HasDurationElapsed()
    {
        return RemainingTime <= 0f;
    }
    
    public bool CanEnterPhase()
    {
        return PhaseEnterPredicate();
    }

    public bool ShouldMoveToNextPhase()
    {
        return ShouldPhaseExit() || HasDurationElapsed();
    }
}