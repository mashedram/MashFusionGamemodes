using LabFusion.Extensions;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using MashGamemodeLibrary.Audio.Players.Extensions;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Basic;

public class AudioPlayer : IRandomAudioPlayer
{
    protected readonly IAudioContainer Container;
    protected readonly AudioSourceProvider SourceProvider;

    public AudioPlayer(IAudioContainer container, AudioSourceProvider sourceProvider)
    {
        Container = container;
        SourceProvider = sourceProvider;
    }

    private IReadOnlyList<string> AudioNames => Container.AudioNames;

    public bool IsPlaying => SourceProvider.IsPlaying;

    protected virtual bool Modifier(AudioSource source)
    {
        return true;
    }

    /// <summary>
    ///     Update the audio player.
    ///     This may be ignored if you do not use any special effects.
    /// </summary>
    /// <param name="delta"></param>
    public void Update(float delta)
    {
        SourceProvider.Update(delta);
    }

    public string GetRandomAudioName()
    {
        return AudioNames.Count > 0 ? AudioNames.GetRandom() : "";
    }

    public void Stop()
    {
        SourceProvider.StopAll();
    }

    public void Play(string name)
    {
        Container.RequestClip(name, clip =>
        {
            if (!clip) return;

            var source = SourceProvider.GetAudioSource();
            
            if (!Modifier(source.Source))
                return;
            
            source.Play(clip!);
        });
    }
}