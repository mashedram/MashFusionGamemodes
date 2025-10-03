using System.Collections.Immutable;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Environment;

class EnvironmentChangePacket : INetSerializable
{
    public ulong StateHash;

    public int? GetSize()
    {
        return sizeof(ulong);
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref StateHash);
    }
}

public class EnvironmentManager<TContext, TInternalContent> 
    where TContext : GameContext
{
    private EnvironmentState<TInternalContent>? _activeState;
    private EnvironmentProfile<TInternalContent>? _profile;
    private Func<TContext, TInternalContent> _contextBuilder;
    private TInternalContent _context = default!;

    public EnvironmentManager(Func<TContext, TInternalContent> contextBuilder)
    {
        _contextBuilder = contextBuilder;
    }
    
    // Local

    private void SetState(EnvironmentState<TInternalContent> state)
    {
        if (_activeState == state)
            return;
        _activeState?.Remove(_context);
        _activeState = state;
        _activeState.Apply(_context);
    }
    
    private void BuildContext()
    {
        var context = GamemodeWithContext<TContext>.Context;
        _context = _contextBuilder(context);
    }

    public void StartPlaying(EnvironmentProfile<TInternalContent> profile)
    {
        _profile = profile;
    }

    public void Stop()
    {
        if (_profile == null)
            return;
        
        _activeState?.Remove(_context);
        _activeState = null;
        _profile.Cleanup();
        _profile = null;
    }
    
    public void Update(float delta)
    {
        if (_profile == null)
            return;
        
        BuildContext();

        _activeState?.Update(_context, delta);

        var wantedState = _profile.GetWantedState(_context);
        if (wantedState == _activeState)
            return;
        
        SetState(wantedState);
    }
}