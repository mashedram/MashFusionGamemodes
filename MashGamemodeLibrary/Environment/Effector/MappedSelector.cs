using System.Collections.Immutable;
using MelonLoader;

namespace MashGamemodeLibrary.Environment.Effector;

public abstract class MappedSelector<TContext, TKey> : EnvironmentEffector<TContext> where TKey : notnull
{
    private readonly ImmutableDictionary<TKey, EnvironmentEffector<TContext>> _effectors;
    private EnvironmentEffector<TContext>? _currentEffector;

    protected MappedSelector()
    {
        var map = new Dictionary<TKey, EnvironmentEffector<TContext>>();
        // We want a nice builder pattern for the map, so we call a virtual method here.
        // ReSharper disable once VirtualMemberCallInConstructor
        BuildMap(ref map);
        _effectors = map.ToImmutableDictionary();
    }

    protected abstract void BuildMap(ref Dictionary<TKey, EnvironmentEffector<TContext>> map);

    protected abstract TKey Selector(TContext context);

    public override void Apply(TContext context)
    {
        _currentEffector?.Apply(context);
    }

    public override bool CanApply(TContext context)
    {
        var key = Selector(context);
        if (!_effectors.TryGetValue(key, out var effector))
        {
            MelonLogger.Error($"MappedSelector: No effector found for key {key}");
            return false;
        }

        _currentEffector = effector;
        return _currentEffector.CanApply(context);
    }

    public override void Update(TContext context, float delta)
    {
        _currentEffector?.Update(context, delta);
    }

    public override void Remove(TContext context)
    {
        _currentEffector?.Remove(context);
    }
}