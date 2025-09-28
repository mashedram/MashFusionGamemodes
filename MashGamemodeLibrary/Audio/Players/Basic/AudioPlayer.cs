using LabFusion.Extensions;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using MashGamemodeLibrary.Audio.Players.Extensions;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Basic;

public class AudioPlayer : IRandomAudioPlayer
{
    public delegate void AudioModifier(ref AudioSource source);
    
    protected readonly IAudioContainer Container;
    protected readonly IAudioSourceProvider SourceProvider;
    
    public AudioPlayer(IAudioContainer container, IAudioSourceProvider sourceProvider)
    {
        Container = container;
        SourceProvider = sourceProvider;
    }

    public List<string> AudioNames => Container.AudioNames;

    public bool IsPlaying => SourceProvider.IsPlaying;
    
    public void Play(string name, AudioModifier? modifier = null)
    {
        Container.RequestClip(name, clip =>
        {
            if (!clip) return;
            
            var source = SourceProvider.GetAudioSource();
            source.clip = clip;
            modifier?.Invoke(ref source);
            source.Play();
        });
    }

    public string GetRandomAudioName()
    {
        return AudioNames.GetRandom();
    }
}