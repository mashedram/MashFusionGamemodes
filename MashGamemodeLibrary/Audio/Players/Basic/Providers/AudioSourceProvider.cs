using MashGamemodeLibrary.Audio.Modifiers;

namespace MashGamemodeLibrary.Audio.Players.Basic.Providers;

public abstract class AudioSourceProvider
{
    private readonly AudioModifierFactory _modifierFactory;

    public AudioSourceProvider(AudioModifierFactory modifierFactory)
    {
        _modifierFactory = modifierFactory;
    }

    public abstract bool IsPlaying { get; }
    protected abstract AudioSourceEntity NextAudioSource(AudioModifierFactory modifierFactory);
    public abstract void StopAll();
    public abstract void Update(float delta);

    public AudioSourceEntity GetAudioSource()
    {
        return NextAudioSource(_modifierFactory);
    }
}