using LabFusion.Extensions;
using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using MashGamemodeLibrary.Audio.Players.Extensions;

namespace MashGamemodeLibrary.Audio.Players.Basic;

public class AudioPlayer : IRandomAudioPlayer, IAudioPlayer
{
    protected readonly IAudioContainer Container;
    protected readonly AudioSourceProvider SourceProvider;

    public AudioPlayer(IAudioContainer container, AudioSourceProvider sourceProvider)
    {
        Container = container;
        SourceProvider = sourceProvider;
    }

    public List<string> AudioNames => Container.AudioNames;

    public bool IsPlaying => SourceProvider.IsPlaying;

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
        return AudioNames.GetRandom();
    }

    public void Play(string name, IAudioModifier? modifier = null)
    {
        Container.RequestClip(name, clip =>
        {
            if (!clip) return;

            var source = SourceProvider.GetAudioSource();
            source.Play(clip!);
        });
    }

    public void StopAll()
    {
        SourceProvider.StopAll();
    }
}