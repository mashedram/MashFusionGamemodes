using MashGamemodeLibrary.Audio.Modifiers;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Players.Basic.Providers;

public abstract class AudioSourceProvider
{
    private readonly AudioModifierFactory _modifierFactory;
    protected abstract AudioSourceEntity NextAudioSource(AudioModifierFactory modifierFactory);
    
    public abstract bool IsPlaying { get; }
    public abstract void StopAll();
    public abstract void Update(float delta);

    public AudioSourceProvider(AudioModifierFactory modifierFactory)
    {
        _modifierFactory = modifierFactory;
    }

    public AudioSourceEntity GetAudioSource()
    {
        return NextAudioSource(_modifierFactory);
    }
}