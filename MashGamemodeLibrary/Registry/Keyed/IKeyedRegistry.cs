using System.Diagnostics.CodeAnalysis;

namespace MashGamemodeLibrary.Registry.Keyed;

public interface IKeyedRegistry<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
    where TValue : notnull
{
    public delegate void OnRegisterHandler(TKey key, TValue value);
    public event OnRegisterHandler? OnRegister;
    
    public void Register<T>(TKey key, T value) where T : TValue;
    public TValue? Get(TKey key);
    public bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value);
    public bool Contains(TKey key);
}