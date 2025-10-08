using MashGamemodeLibrary.Audio.Modifiers;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Basic.Providers;

public class SingleAudioSourceProvider : AudioSourceProvider
{
    private AudioSourceEntity? _audioSource;

    public SingleAudioSourceProvider(AudioModifierFactory modifierFactory) : base(modifierFactory)
    {
    }

    public override bool IsPlaying => _audioSource && _audioSource!.IsPlaying;

    protected override AudioSourceEntity NextAudioSource(AudioModifierFactory modifierFactory)
    {
        if (_audioSource)
            return _audioSource!;

        _audioSource = new AudioSourceEntity(modifierFactory);
        return _audioSource;
    }

    public override void StopAll()
    {
        if (_audioSource)
            _audioSource!.Stop();
    }
    
    public override void Update(float delta)
    {
        if (_audioSource)
            _audioSource!.Update(delta);
    }
}