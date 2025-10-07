namespace MashGamemodeLibrary.Audio.Modifiers;

public class AudioModifierFactory
{
    public delegate T ModifierDelegate<T>(T source) where T : IAudioModifier;
    private delegate IAudioModifier ModifierDelegateInternal();
    private readonly List<ModifierDelegateInternal> _registeredModifiers = new();
    
    public AudioModifierFactory AddModifier<T>(ModifierDelegate<T>? factory = null) where T : IAudioModifier, new()
    {
        _registeredModifiers.Add(() =>
        {
            var modifier = new T();
            return factory == null ? modifier : factory(modifier);
            ;
        });
        return this;
    }

    public HashSet<IAudioModifier> Build()
    {
        return _registeredModifiers.Select(func => func()).ToHashSet();
    }
}