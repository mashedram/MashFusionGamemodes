using System.Diagnostics.CodeAnalysis;
using LabFusion.Extensions;
using MashGamemodeLibrary.Audio.Modifiers;

namespace MashGamemodeLibrary.Audio.Players.Basic.Providers;

internal class AudioPoolMember
{
    private AudioSourceEntity? _source;

    public AudioSourceEntity GetOrCreate(AudioModifierFactory factory)
    {
        if (_source is { IsValid: true })
            return _source!;

        _source = new AudioSourceEntity(factory);
        return _source;
    }

    public bool TryGet([MaybeNullWhen(false)] out AudioSourceEntity audioSource)
    {
        audioSource = _source;
        return _source;
    }

    public void Update(float delta)
    {
        if (_source is { IsValid: true })
            _source.Update(delta);
    }
}

public class PooledAudioSourceProvider : AudioSourceProvider
{
    private readonly int _poolSize;
    private readonly AudioPoolMember[] _sources;
    private int _currentIndex;

    public PooledAudioSourceProvider(int size, AudioModifierFactory modifierFactory) : base(modifierFactory)
    {
        _poolSize = size;
        _sources = Enumerable.Range(0, _poolSize)
            .Select(_ => new AudioPoolMember())
            .ToArray();
    }

    public override bool IsPlaying => _sources.Any(x => x.TryGet(out var entity) && entity.IsPlaying);

    protected override AudioSourceEntity NextAudioSource(AudioModifierFactory modifierFactory)
    {
        var timeout = 0;

        AudioSourceEntity source;
        do
        {
            _currentIndex = (_currentIndex + 1) % _poolSize;
            source = _sources[_currentIndex].GetOrCreate(modifierFactory);

            timeout++;
            if (timeout >= _poolSize)
                break;
        } while (source.IsPlaying);

        return source;
    }

    public override void StopAll()
    {
        foreach (var member in _sources)
            if (member.TryGet(out var source) && source.IsPlaying)
                source.Stop();
    }

    public override void Update(float delta)
    {
        _sources.ForEach(source => source.Update(delta));
    }
}