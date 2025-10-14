using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Environment.State;

public abstract class EnvironmentState<TContext>
{
    public EnvironmentState(EnvironmentEffector<TContext>[] effectors)
    {
        StateHash = (GetType().FullName ?? throw new InvalidOperationException()).GetStableHash();

        Effectors = effectors;
    }

    public ulong StateHash { get; init; }

    public abstract int Priority { get; }
    public virtual int Layer => 0;
    public EnvironmentEffector<TContext>[] Effectors { get; }

    public abstract bool CanPlay(TContext context);
}