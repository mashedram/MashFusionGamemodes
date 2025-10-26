using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Util.Timer;

namespace MashGamemodeLibrary.Phase;

public abstract class GamePhase
{
    public abstract string Name { get; }
    public bool IsActive { get; private set; }

    // Implementation

    private float? _internalDuration;
    public abstract float Duration { get; }

    protected virtual TimeMarker[] Markers { get; } = Array.Empty<TimeMarker>();
    private MarkableTimer? _timer;
    public float ElapsedTime => _timer?.GetElapsedTime() ?? 0f;

    public bool HasReachedDuration()
    {
        return _timer?.HasReachedTimeout() ?? false;
    }

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
        if (!Equals(_internalDuration, Duration))
        {
            _internalDuration = Duration;
            _timer?.SetTimeout(Duration);
        }
        
        _timer?.Update(delta);
        OnUpdate();
    }

    public void Enter()
    {
        IsActive = true;
        
        _internalDuration = Duration;
        _timer ??= new MarkableTimer(Duration, Markers);
        _timer.Reset();
      
        OnPhaseEnter();
    }

    public void Exit()
    {
        IsActive = false;
        OnPhaseExit();
    }
}