using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Basic.Providers;

class AudioPoolMember
{
    private AudioSource? _source;

    public AudioSource GetOrCreate()
    {
        if (_source)
            return _source!;
        
        var go = new GameObject("PooledAudioSource");
        _source = go.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        
        return _source;
    }
}

public class PooledAudioSourceProvider : IAudioSourceProvider
{
    private readonly int _poolSize;
    private int _currentIndex;
    private readonly AudioPoolMember[] _sources;

    public PooledAudioSourceProvider(int size)
    {
        _poolSize = size;
        _sources = Enumerable.Range(0, _poolSize)
            .Select(_ => new AudioPoolMember())
            .ToArray();
    }

    public bool IsPlaying => _sources.Any(x => x.GetOrCreate().isPlaying);

    public AudioSource GetAudioSource()
    {
        var timeout = 0;
        
        AudioSource source;
        do
        {
            _currentIndex = (_currentIndex + 1) % _poolSize;
            source = _sources[_currentIndex].GetOrCreate();

            timeout++;
            if (timeout >= _poolSize)
                break;
        } while (source.isPlaying);

        return source;
    }
}