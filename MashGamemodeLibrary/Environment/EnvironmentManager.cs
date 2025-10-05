using System.Collections.Immutable;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Environment.State;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking;
using MashGamemodeLibrary.Util;
using MelonLoader;

namespace MashGamemodeLibrary.Environment;

public class Track<TInternalContext>
{
    private Enum _id;
    private EnvironmentEffector<TInternalContext>? _effector;

    public Track(Enum id)
    {
        _id = id;
    }
    
    public void SetEffector(EnvironmentEffector<TInternalContext>? effector, TInternalContext context)
    {
        if (_effector == effector)
            return;
        
        _effector?.Remove(context);
        _effector = effector;
        _effector?.Apply(context);
    }

    public void Update(TInternalContext context, float delta)
    {
        _effector?.Update(context, delta);
    }
}

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

public class EnvironmentManager<TContext, TInternalContext> 
    where TContext : GameContext
{
    private ImmutableSortedSet<int> _knownLayers = null!;
    private Dictionary<int, EnvironmentState<TInternalContext>?> _activeStates = new();
    private Dictionary<Enum, Track<TInternalContext>> _tracks = new();
    private EnvironmentProfile<TInternalContext>? _profile;
    private Func<TContext, TInternalContext> _contextBuilder;
    private TInternalContext _context = default!;

    public EnvironmentManager(Func<TContext, TInternalContext> contextBuilder)
    {
        _contextBuilder = contextBuilder;
    }
    
    // Local
    
    private Track<TInternalContext> GetOrCreateTrack(Enum id)
    {
        if (_tracks.TryGetValue(id, out var value))
            return value;
        
        var track = new Track<TInternalContext>(id);
        _tracks[id] = track;
        return track;
    }
    
    private void BuildContext()
    {
        var context = GamemodeWithContext<TContext>.Context;
        _context = _contextBuilder(context);
    }

    public void StartPlaying(EnvironmentProfile<TInternalContext> profile)
    {
        _profile = profile;
        _knownLayers = profile.GetAllStates()
            .Select(e => e.Layer)
            .ToImmutableSortedSet(Comparer<int>.Create((a, b) => b.CompareTo(a)));
    }

    public void Stop()
    {
        if (_profile == null)
            return;
        
        _tracks.Values.ForEach(track => track.SetEffector(null, _context));
        _tracks.Clear();
        _activeStates.Clear();
        _profile.Cleanup();
        _profile = null;
    }
    
    // TODO: Make layers correctly work within the music system
    public void Update(float delta)
    {
        if (_profile == null)
            return;
        
        BuildContext();
        
        // Update existing tracks
        _tracks.Values.ForEach(track => track.Update(_context, delta));
        
        var assignedTracks = new HashSet<Enum>();

        foreach (var layerId in _knownLayers)
        {
            // Get the active state for this layer
            var wantedState = _profile.GetWantedState(layerId, _context);
            var activeState = _activeStates.GetValueOrDefault(layerId);
            if (ReferenceEquals(wantedState, activeState))
                continue;
            _activeStates[layerId] = wantedState;
            
            if (wantedState == null)
                continue;

            foreach (var effector in wantedState.Effectors)
            {
                var trackId = effector.Track;
                // If we assigned this track already, skip it
                // Keeps priority based track assignment
                if (!effector.CanApply(_context))
                    continue;
                if (assignedTracks.Contains(trackId))
                    continue;
                var track = GetOrCreateTrack(trackId);
                track.SetEffector(effector, _context);
                assignedTracks.Add(trackId);
            }
        }
    }
}