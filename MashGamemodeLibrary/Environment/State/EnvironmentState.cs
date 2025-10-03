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
    public abstract bool CanPlay(TContext context);
    
    public void Apply(TContext context)
    {
        foreach (var effector in _effectors)
        {
            if (!effector.CanApply(context)) continue;
            effector.Apply(context);
        }
    }
    
    public void Update(TContext context, float delta)
    {
        foreach (var effector in _effectors)
        {
            effector.Update(context, delta);
        }
    }
    
    public void Remove(TContext context)
    {
        foreach (var effector in _effectors)
        {
            effector.Remove(context);
        }
    }
}