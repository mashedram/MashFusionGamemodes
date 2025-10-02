using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Basic.Providers;

public class SingleAudioSourceProvider : IAudioSourceProvider
{
    private AudioSource? _audioSource;

    public bool IsPlaying => _audioSource && _audioSource!.isPlaying;

    public AudioSource GetAudioSource()
    {
        if (_audioSource)
            return _audioSource!;
        
        var go = new GameObject("AudioPlayer");
        _audioSource = go.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        
        return _audioSource;
    }

    public void StopAll()
    {
        if (_audioSource)
            _audioSource!.Stop();
    }
}