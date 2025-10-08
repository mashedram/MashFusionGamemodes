using System.Collections.Immutable;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Environment;

class EnvironmentStateComparer<TInternalContent> : IComparer<EnvironmentState<TInternalContent>>
{
    public int Compare(EnvironmentState<TInternalContent>? x, EnvironmentState<TInternalContent>? y)
    {
        var xPriority = x?.Priority ?? 0;
        var yPriority = y?.Priority ?? 0;
        
        return yPriority.CompareTo(xPriority);
    }
}

public class EnvironmentProfile<TInternalContent>
{
    private readonly string _name;
    private readonly ImmutableDictionary<ulong, EnvironmentState<TInternalContent>> _stateLookup;
    private readonly ImmutableSortedSet<EnvironmentState<TInternalContent>> _states;
    
    private readonly Action _cleanupAction;
    
    public string Name => _name;

    public EnvironmentProfile(string name, EnvironmentState<TInternalContent>[] states, Action cleanupAction)
    {
        _name = name;
        _stateLookup = states.ToImmutableDictionary(state => (state.GetType().FullName ?? throw new InvalidOperationException()).GetStableHash());
        _states = states.ToImmutableSortedSet(new EnvironmentStateComparer<TInternalContent>());
        
        _cleanupAction = cleanupAction;
    }


    public EnvironmentState<TInternalContent>? GetStateByHash(ulong hash)
    {
        return _stateLookup.GetValueOrDefault(hash);
    }
    
    public ImmutableSortedSet<EnvironmentState<TInternalContent>> GetAllStates()
    {
        return _states;
    }
    
    public EnvironmentState<TInternalContent>? GetWantedState(int layer, TInternalContent context)
    {
        return _states
            .Where(e => e.Layer == layer)
            .FirstOrDefault(musicState => musicState != null && musicState.CanPlay(context), null);
    }
    
    public void Cleanup()
    {
        _cleanupAction();
    }
}