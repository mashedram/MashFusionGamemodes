using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace MashGamemodeLibrary.Registry.Keyed;

public class KeyedRegistry<TKey, TValue> : IKeyedRegistry<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    private readonly Dictionary<TKey, TValue> _dictionary = new();

    public event IKeyedRegistry<TKey, TValue>.OnRegisterHandler? OnRegister;
    public void Register<T>(TKey id, T value) where T : TValue
    {
        _dictionary.Add(id, value);

        OnRegister?.Invoke(id, value);
    }

    public TValue? Get(TKey id)
    {
        return _dictionary.GetValueOrDefault(id);
    }

    public bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public bool Contains(TKey id)
    {
        return _dictionary.ContainsKey(id);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}