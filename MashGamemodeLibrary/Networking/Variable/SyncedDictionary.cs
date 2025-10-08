using System.Diagnostics.CodeAnalysis;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Execution;
using Microsoft.VisualBasic;

namespace MashGamemodeLibrary.networking.Variable;

public class Pair<TKey, TValue>
    where TKey: notnull
    where TValue : struct
{
    public TKey Key;
    public TValue? Value;

    public Pair(TKey key, TValue? value)
    {
        Key = key;
        Value = value;
    }
}

// TODO: Bricks on latejoin
public abstract class SyncedDictionary<TKey, TValue> : GenericRemoteEvent<Pair<TKey, TValue>> 
    where TKey : notnull 
    where TValue : struct
{
    public delegate void ValueChangedHandler(TKey key, TValue value);
    public delegate void ValueRemovedHandler(TKey key, TValue oldValue);
    public event ValueChangedHandler? OnValueChanged;
    public event ValueRemovedHandler? OnValueRemoved;
    
    
    private readonly Dictionary<TKey, TValue> _dictionary = new();

    protected SyncedDictionary(string name) : base(name)
    {
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
        MultiplayerHooking.OnJoinedServer += OnServerChanged;
        MultiplayerHooking.OnDisconnected += OnServerChanged;
    }
    
    ~SyncedDictionary()
    {
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoined;
        MultiplayerHooking.OnJoinedServer -= OnServerChanged;
        MultiplayerHooking.OnDisconnected -= OnServerChanged;
    }
    
    private void OnPlayerJoined(PlayerID playerId)
    {
        Executor.RunIfHost(() =>
        {
            foreach (var (key, value) in _dictionary)
            {
                Relay(new Pair<TKey, TValue>(key, value));
            }
        });
    }
    
    private void OnServerChanged()
    {
        _dictionary.Clear();
    }
    
    // Private methods
    
    private void SetValue(TKey key, TValue value, bool sendUpdate)
    {
        _dictionary[key] = value;
        OnValueChanged?.Invoke(key, value);
        
        if (sendUpdate)
            Relay(new Pair<TKey, TValue>(key, value));
    }
    
    private bool RemoveValue(TKey key, bool sendUpdate)
    {
        if (!_dictionary.Remove(key, out var oldValue)) return false;
        OnValueRemoved?.Invoke(key, oldValue);
        if (sendUpdate)
            Relay(new Pair<TKey, TValue>(key, null));
        return true;
    }
    
    public void Clear()
    {
        var keys = _dictionary.Keys.ToList();
        foreach (var key in keys)
        {
            RemoveValue(key, true);
        }
    }
    
    // Setters and Getters

    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set => SetValue(key, value, true);
    }

    public bool Remove(TKey key)
    {
        return RemoveValue(key, true);
    }
    
    public bool TryGetValue(TKey key, [MaybeNullWhen(returnValue: false)] out TValue value)
    {
        return _dictionary.TryGetValue(key, out value);
    }
    
    // Abstract methods for serialization
    
    protected abstract void WriteKey(NetWriter writer, TKey key);
    protected abstract TKey ReadKey(NetReader reader);
    protected abstract void WriteValue(NetWriter writer, TValue value);
    protected abstract TValue ReadValue(NetReader reader);

    protected override void Write(NetWriter writer, Pair<TKey, TValue> data)
    {
        WriteKey(writer, data.Key);
        if (data.Value.HasValue)
        {
            writer.Write(true);
            WriteValue(writer, data.Value.Value);
        }
        else
        {
            writer.Write(false);
        }
    }

    protected override void Read(NetReader reader)
    {
        var key = ReadKey(reader);
        if (reader.ReadBoolean())
        {
            var value = ReadValue(reader);
            SetValue(key, value, false);
        }
        else
        {
            RemoveValue(key, false);
        }
    }
}