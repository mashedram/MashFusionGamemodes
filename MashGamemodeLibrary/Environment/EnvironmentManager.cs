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
    where TInternalContent : INetSerializable, new()
{
    private readonly RemoteEvent<EnvironmentChangePacket> _changePacket;
    
    private EnvironmentState<TInternalContent>? _activeState;
    private EnvironmentProfile<TInternalContent> _profile;
    private Func<TContext, TInternalContent> _contextBuilder;
    private TInternalContent _context = default!;
    private bool _isActive;

    public EnvironmentManager(EnvironmentProfile<TInternalContent> profile, Func<TContext, TInternalContent> contextBuilder)
    {
        _profile = profile;
        _contextBuilder = contextBuilder;

        var name = $"MGL_ENV_{typeof(TContext).Name}_{typeof(TInternalContent).Name}";
        _changePacket = new RemoteEvent<EnvironmentChangePacket>(name, OnChangePacket, false);
    }
    
    // Local

    private void SetState(EnvironmentState<TInternalContent> state)
    {
        if (_activeState == state)
            return;
        _activeState?.Remove(_context);
        _activeState = state;
        _activeState.Apply(_context);
        
        Executor.RunIfHost(Sync);   
    }
    
    private void BuildContext()
    {
        var context = GamemodeWithContext<TContext>.Context;
        _context = _contextBuilder(context);
    }

    private void Sync()
    {
        _changePacket.Call(new EnvironmentChangePacket
        {
            StateHash = _activeState?.StateHash ?? 0
        });
    }

    public void Update(float delta)
    {
        if (!_isActive)
            return;
        
        BuildContext();

        _activeState?.Update(_context, delta);

        var wantedState = _profile.GetWantedState(_context);
        if (wantedState == _activeState)
            return;
        
        SetState(wantedState);
    }
    
    // Remote Events

    private void OnChangePacket(EnvironmentChangePacket packet)
    {
        if (_profile == null)
        {
            MelonLogger.Error("Received environment change packet but no profile is set!");
            return;
        }
        
        if (packet.StateHash == 0)
        {
            _profile.Cleanup();
            return;
        }

        var state = _profile.GetStateByHash(packet.StateHash);
        if (state == null)
        {
            MelonLogger.Error($"Received environment change packet but state with hash {packet.StateHash} was not found in profile {_profile.Name}!");
            return;
        }

        BuildContext();
        SetState(state);
    }
}