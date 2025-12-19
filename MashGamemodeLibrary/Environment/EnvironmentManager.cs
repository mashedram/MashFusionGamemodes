using System.Collections.Immutable;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Context;
using MashGamemodeLibrary.Context.Control;

namespace MashGamemodeLibrary.Environment;

public class Track<TInternalContext>
{
    private EnvironmentEffector<TInternalContext>? _effector;
    private Enum _id;

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

internal class EnvironmentChangePacket : INetSerializable
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

public class EnvironmentManager<TContext, TInternalContext> : IUpdating, IContextfull<TContext>, IStoppable
    where TContext : GameModeContext<TContext>, new()
{
    private readonly Func<TContext, TInternalContext> _contextBuilder;
    private readonly Dictionary<Enum, Track<TInternalContext>> _tracks = new();
    private TInternalContext _context = default!;
    private ImmutableSortedSet<int> _knownLayers = null!;
    private EnvironmentProfile<TInternalContext>? _profile;

    public EnvironmentManager(Func<TContext, TInternalContext> contextBuilder)
    {
        _contextBuilder = contextBuilder;
    }

    public void SetContext(TContext context)
    {
        _context = _contextBuilder(context);
    }

    public void Stop()
    {
        if (_profile == null)
            return;

        _tracks.Values.ForEach(track => track.SetEffector(null, _context));
        _tracks.Clear();
        _profile.Cleanup();
        _profile = null;
    }

    public void Update(float delta)
    {
        if (_profile == null)
            return;

        // Update existing tracks
        _tracks.Values.ForEach(track => track.Update(_context, delta));

        var assignedTracks = new HashSet<Enum>();

        foreach (var layerId in _knownLayers)
        {
            // Get the active state for this layer
            var wantedState = _profile.GetWantedState(layerId, _context);

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

    // Local

    private Track<TInternalContext> GetOrCreateTrack(Enum id)
    {
        if (_tracks.TryGetValue(id, out var value))
            return value;

        var track = new Track<TInternalContext>(id);
        _tracks[id] = track;
        return track;
    }

    public void StartPlaying(EnvironmentProfile<TInternalContext> profile)
    {
        _profile = profile;
        _knownLayers = profile.GetAllStates()
            .Select(e => e.Layer)
            .ToImmutableSortedSet(Comparer<int>.Create((a, b) => b.CompareTo(a)));
    }
}