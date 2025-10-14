using LabFusion.Extensions;
using MashGamemodeLibrary.Audio.Modifiers;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Basic.Providers;

public class AudioSourceEntity
{
    public readonly HashSet<IAudioModifier> Modifiers = new();
    private AudioSource _source;

    public AudioSourceEntity(AudioModifierFactory modifierFactory)
    {
        var go = new GameObject("AudioSourceEntity");
        _source = go.AddComponent<AudioSource>();
        _source.playOnAwake = false;

        Modifiers = modifierFactory.Build();
    }

    public AudioSourceEntity(AudioSource source, AudioModifierFactory modifierFactory)
    {
        _source = source;
        Modifiers = modifierFactory.Build();
    }

    public AudioSource Source => _source;
    public ref AudioSource SourceRef => ref _source;

    public bool IsValid => Source;
    public bool IsPlaying => Source.isPlaying;
    public Transform Transform => Source.transform;

    public static implicit operator bool(AudioSourceEntity? entity)
    {
        return entity is { IsValid: true };
    }

    public void Play(AudioClip clip)
    {
        Modifiers.ForEach(modifier => modifier.OnStart(ref _source));

        Source.clip = clip;
        Source.Play();
    }

    public void Update(float delta)
    {
        Modifiers.ForEach(modifier => modifier.Update(ref _source, delta));
    }

    public void Stop()
    {
        Source.Stop();
    }
}