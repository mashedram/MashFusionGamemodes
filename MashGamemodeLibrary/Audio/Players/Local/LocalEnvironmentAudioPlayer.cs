using MashGamemodeLibrary.Audio.Containers;
using MashGamemodeLibrary.Audio.Modifiers;
using MashGamemodeLibrary.Audio.Players.Basic;
using MashGamemodeLibrary.Audio.Players.Basic.Providers;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Local;

public class LocalEnvironmentAudioPlayer : AudioPlayer
{
    public LocalEnvironmentAudioPlayer(IAudioContainer container, AudioModifierFactory factory) : base(
        container, new SingleAudioSourceProvider(factory))
    {
    }
    
    public LocalEnvironmentAudioPlayer(IAudioContainer container, AudioSourceProvider sourceProvider) : base(
        container, sourceProvider)
    {
    }

    protected override bool Modifier(AudioSource source)
    {
        source.spatialBlend = 1f;
        source.gameObject.transform.position = BoneLib.Player.Head.position + UnityEngine.Random.onUnitSphere * UnityEngine.Random.RandomRange(10f, 30f);
        source.rolloffMode = AudioRolloffMode.Custom;
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Constant(0f, 1f, 1f));
        source.maxDistance = 40f;
        
        return true;
    }

    public void PlayRandom()
    {
        var name = GetRandomAudioName();
        if (string.IsNullOrEmpty(name)) return;

        Play(name);
    }
}