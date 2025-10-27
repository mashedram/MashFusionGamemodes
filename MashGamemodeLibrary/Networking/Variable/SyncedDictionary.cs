using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable.Encoder;

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

public class SyncedDictionary<TKey, TValue> : GenericRemoteEvent<DictionaryEdit<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, ICatchup,
    IResettable
    where TKey : notnull
    where TValue : notnull
{
    public delegate void ValueChangedHandler(TKey key, TValue value);
    public delegate void ValueRemovedHandler(TKey key, TValue oldValue);

    public delegate void ValueClearHandler();

    private readonly IEncoder<TKey> _keyEncoder;
    private readonly IEncoder<TValue> _valueEncoder;
    private readonly IRefEncoder<TValue>? _refEncoder;
    private readonly Dictionary<TKey, TValue> _dictionary = new();


    public SyncedDictionary(string name, IEncoder<TKey> keyEncoder, IEncoder<TValue> valueEncoder) : base(name, CommonNetworkRoutes.HostToAll)
    {
        _keyEncoder = keyEncoder;
        _valueEncoder = valueEncoder;

        _refEncoder = valueEncoder as IRefEncoder<TValue>;

        MultiplayerHooking.OnJoinedServer += ClearLocal;
    }

    ~SyncedDictionary()
    {
        MultiplayerHooking.OnJoinedServer -= ClearLocal;
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

    public event ValueChangedHandler? OnValueAdded;
    public event ValueChangedHandler? OnValueChanged;
    public event ValueRemovedHandler? OnValueRemoved;
    public event ValueClearHandler? OnValueCleared;

    // Private methods

    private void SetValue(TKey key, TValue value, bool sendUpdate)
    {
        _dictionary[key] = value;
        OnValueAdded?.Invoke(key, value);   

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
        var removed = _dictionary.ToImmutableDictionary();
        _dictionary.Clear();
        removed.ForEach(pair => OnValueRemoved?.Invoke(pair.Key, pair.Value));
        OnValueCleared?.Invoke();
        
        if (sendUpdate)
            Relay(DictionaryEdit<TKey, TValue>.Clear());
    }

    private void ClearLocal()
    {
        Clear(false);
    }

    public bool Remove(TKey key)
    {
        return RemoveValue(key, true);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }
    
    public TValue? GetValueOrDefault(TKey key, TValue d = default!)
    {
        return _dictionary.GetValueOrDefault(key, d);
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

    public void Sync(TKey key)
    {
        if (!TryGetValue(key, out var value))
            return;
        
        OnValueChanged?.Invoke(key, value);
        Relay(DictionaryEdit<TKey, TValue>.Set(key, value));
    }
    

    // Abstract methods for serialization
    
    protected override int? GetSize(DictionaryEdit<TKey, TValue> data)
    {
        var size = sizeof(DictionaryEditType);
        if (data.Type is DictionaryEditType.Set or DictionaryEditType.Remove) size += _keyEncoder.GetSize(data.Key);
        if (data.Type is DictionaryEditType.Set) size += _valueEncoder.GetSize(data.Value);
        return size;
    }
    
    protected override void Write(NetWriter writer, DictionaryEdit<TKey, TValue> data)
    {
        writer.Write(data.Type);
        if (data.Type is DictionaryEditType.Set or DictionaryEditType.Remove) _keyEncoder.Write(writer, data.Key);
        
        if (data.Type is not DictionaryEditType.Set)
            return;

        if (_refEncoder == null)
        {
            _valueEncoder.Write(writer, data.Value);
            return;
        }
        
        _refEncoder.Write(writer, data.Value);
    }

    private void SetOrUpdate(NetReader reader)
    {
        var setKey = _keyEncoder.Read(reader);
        if (_refEncoder != null && TryGetValue(setKey, out var value))
        {
            _refEncoder.Serialize(reader, value);
            OnValueChanged?.Invoke(setKey, value);
            return;
        }

        var readValue = _valueEncoder.Read(reader);
        SetValue(setKey, readValue, false);
    }

    protected override void Read(byte playerId, NetReader reader)
    {
        var type = reader.ReadEnum<DictionaryEditType>();
        switch (type)
        {
            case DictionaryEditType.Set:
                SetOrUpdate(reader);
                break;
            case DictionaryEditType.Remove:
                var removeKey = _keyEncoder.Read(reader);
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