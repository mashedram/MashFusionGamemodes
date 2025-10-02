using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Basic.Providers;

public interface IAudioSourceProvider
{
    bool IsPlaying { get; }
    AudioSource GetAudioSource();
    void StopAll();
}