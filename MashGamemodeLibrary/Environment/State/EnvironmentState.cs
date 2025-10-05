using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Environment.State;

public abstract class EnvironmentState<TContext>
{
    public ulong StateHash { get; init; }
    private EnvironmentEffector<TContext>[] _effectors;

    public EnvironmentState(EnvironmentEffector<TContext>[] effectors)
    {
        StateHash = (GetType().FullName ?? throw new InvalidOperationException()).GetStableHash();
        
        _effectors = effectors;
    }
    
    public abstract int Priority { get; }
    public virtual int Layer => 0;
    public EnvironmentEffector<TContext>[] Effectors => _effectors;
    public abstract bool CanPlay(TContext context);
    
}