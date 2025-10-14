using System.Collections;
using System.Diagnostics.CodeAnalysis;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;

namespace MashGamemodeLibrary.networking.Variable;

public enum DictionaryEditType
{
    Set,
    Remove,
    Clear
}

public class DictionaryEdit<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    public TKey Key;
    public DictionaryEditType Type;
    public TValue Value;

    private DictionaryEdit(DictionaryEditType type, TKey key, TValue value)
    {
        Type = type;
        Key = key;
        Value = value;
    }

    public static DictionaryEdit<TKey, TValue> Set(TKey key, TValue value)
    {
        return new DictionaryEdit<TKey, TValue>(DictionaryEditType.Set, key, value);
    }

    public static DictionaryEdit<TKey, TValue> Remove(TKey key)
    {
        return new DictionaryEdit<TKey, TValue>(DictionaryEditType.Remove, key, default!);
    }

    public static DictionaryEdit<TKey, TValue> Clear()
    {
        return new DictionaryEdit<TKey, TValue>(DictionaryEditType.Clear, default!, default!);
    }
}

// TODO: Bricks on latejoin
public abstract class SyncedDictionary<TKey, TValue> : GenericRemoteEvent<DictionaryEdit<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, ICatchup,
    IResettable
    where TKey : notnull
    where TValue : notnull
{
    public delegate void ValueChangedHandler(TKey key, TValue value);

    public delegate void ValueRemovedHandler(TKey key, TValue oldValue);


    private readonly Dictionary<TKey, TValue> _dictionary = new();


    protected SyncedDictionary(string name, INetworkRoute? route = null) : base(name, route)
    {
    }

    // Setters and Getters

    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set => SetValue(key, value, true);
    }

    public void OnCatchup(PlayerID playerId)
    {
        Executor.RunIfHost(() =>
        {
            foreach (var (key, value) in _dictionary) Relay(DictionaryEdit<TKey, TValue>.Set(key, value));
        });
    }

    public void Reset()
    {
        _dictionary.Clear();
    }

    public event ValueChangedHandler? OnValueChanged;
    public event ValueRemovedHandler? OnValueRemoved;

    // Private methods

    private void SetValue(TKey key, TValue value, bool sendUpdate)
    {
        _dictionary[key] = value;
        OnValueChanged?.Invoke(key, value);

        if (sendUpdate)
            Relay(DictionaryEdit<TKey, TValue>.Set(key, value));
    }

    private bool RemoveValue(TKey key, bool sendUpdate)
    {
        if (!_dictionary.Remove(key, out var oldValue)) return false;

        OnValueRemoved?.Invoke(key, oldValue);
        if (sendUpdate)
            Relay(DictionaryEdit<TKey, TValue>.Remove(key));
        return true;
    }

    public void Clear(bool sendUpdate = true)
    {
        _dictionary.ForEach(pair => OnValueRemoved?.Invoke(pair.Key, pair.Value));
        _dictionary.Clear();
        if (sendUpdate)
            Relay(DictionaryEdit<TKey, TValue>.Clear());
    }

    public bool Remove(TKey key)
    {
        return RemoveValue(key, true);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public bool ContainsKey(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    public Dictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;
    public Dictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    

    // Abstract methods for serialization

    protected abstract void WriteKey(NetWriter writer, TKey key);
    protected abstract TKey ReadKey(NetReader reader);
    protected abstract void WriteValue(NetWriter writer, TValue value);
    protected abstract TValue ReadValue(NetReader reader, TKey key);

    protected override void Write(NetWriter writer, DictionaryEdit<TKey, TValue> data)
    {
        writer.Write(data.Type);
        if (data.Type == DictionaryEditType.Set || data.Type == DictionaryEditType.Remove) WriteKey(writer, data.Key);

        if (data.Type == DictionaryEditType.Set) WriteValue(writer, data.Value);
    }

    protected override void Read(byte playerId, NetReader reader)
    {
        var type = reader.ReadEnum<DictionaryEditType>();
        switch (type)
        {
            case DictionaryEditType.Set:
                var setKey = ReadKey(reader);
                var value = ReadValue(reader, setKey);
                SetValue(setKey, value, false);
                break;
            case DictionaryEditType.Remove:
                var removeKey = ReadKey(reader);
                RemoveValue(removeKey, false);
                break;
            case DictionaryEditType.Clear:
                Clear(false);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}