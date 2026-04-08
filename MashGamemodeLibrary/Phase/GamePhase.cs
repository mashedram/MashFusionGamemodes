using Il2CppSLZ.Marrow.Interaction;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.Util.Timer;

namespace MashGamemodeLibrary.Phase;

public abstract class GamePhase
{

    // Implementation

    private MarkableTimer? _timer;
    private float? InternalDuration => _timer?.Duration;
    public abstract string Name { get; }
    public bool IsActive { get; private set; }
    public abstract float Duration { get; }

    protected virtual TimeMarker[] Markers { get; } = Array.Empty<TimeMarker>();
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

    public virtual bool CanTimerTick()
    {
        return true;
    }

    public void Update(float delta)
    {
        if (_timer != null && !Equals(InternalDuration, Duration))
        {
            _timer?.SetTimeout(Duration);
        }

        if (CanTimerTick())
            _timer?.Update(delta);
        Executor.RunUnchecked(OnUpdate);
    }

    public void Enter()
    {
        IsActive = true;

        _timer ??= new MarkableTimer(Duration, Markers);
        _timer.Reset();

        Executor.RunUnchecked(OnPhaseEnter);
    }

    public void Exit()
    {
        IsActive = false;
        Executor.RunUnchecked(OnPhaseExit);
    }
}