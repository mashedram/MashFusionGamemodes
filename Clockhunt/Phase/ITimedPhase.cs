namespace Clockhunt.Phase;

public interface ITimedPhase
{
    float ElapsedTime { get; }
    float Duration { get; }
}