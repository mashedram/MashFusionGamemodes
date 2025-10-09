using System.Collections;
using System.Diagnostics.CodeAnalysis;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Utilities;
using MashGamemodeLibrary.Execution;

namespace MashGamemodeLibrary.networking.Variable;

public enum ChangeType
{
    Add,
    Remove,
    Clear
}

public class ChangePacket<T>
{
    public ChangeType Type;
    public T Value;
}

public abstract class SyncedSet<TValue> : GenericRemoteEvent<ChangePacket<TValue>>, IEnumerable<TValue>
{
     public delegate void ValueChangedHandler(TValue value);
    public delegate void ValueRemovedHandler(TValue oldValue);
    public event ValueChangedHandler? OnValueChanged;
    public event ValueRemovedHandler? OnValueRemoved;
    
    
    private readonly HashSet<TValue> _set = new();

    protected SyncedSet(string name) : base(name)
    {
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoined;
        MultiplayerHooking.OnJoinedServer += OnServerChanged;
        MultiplayerHooking.OnDisconnected += OnServerChanged;
    }

    ~SyncedSet()
    {
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoined;
        MultiplayerHooking.OnJoinedServer -= OnServerChanged;
        MultiplayerHooking.OnDisconnected -= OnServerChanged;
    }

    private void RelayAdd(TValue value)
    {
        Relay(new ChangePacket<TValue>
        {
            Type = ChangeType.Add,
            Value = value
        });
    }
    
    private void RelayRemove(TValue value)
    {
        Relay(new ChangePacket<TValue>
        {
            Type = ChangeType.Remove,
            Value = value
        });
    }
    
    private void OnPlayerJoined(PlayerID playerId)
    {
        Executor.RunIfHost(() =>
        {
            foreach (var value in _set)
            {
                RelayAdd(value);
            }
        });
    }
    
    private void OnServerChanged()
    {
        _set.Clear();
    }
    
    // Private methods
    
    private void AddValue(TValue value, bool sendUpdate)
    {
        if (!_set.Add(value)) return;
        OnValueChanged?.Invoke(value);
        
        if (sendUpdate)
            RelayAdd(value);
    }
    
    private bool RemoveValue(TValue value, bool sendUpdate)
    {
        if (!_set.Remove(value)) return false;
        OnValueRemoved?.Invoke(value);
        if (sendUpdate)
            RelayRemove(value);
        return true;
    }
    
    public void Clear(bool sendUpdate = true)
    {
        _set.Clear();

        if (sendUpdate)
        {
            Relay(new ChangePacket<TValue>
            {
                Type = ChangeType.Clear,
                Value = default!
            });
        }
    }
    
    // Setters and Getters

    public void Add(TValue value)
    {
        AddValue(value, true);
    }

    public void Remove(TValue value)
    {
        RemoveValue(value, true);
    }

    public int Count => _set.Count;
    
    public IEnumerator<TValue> GetEnumerator()
    {
        return _set.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    // Abstract methods for serialization
    
    protected abstract void WriteValue(NetWriter writer, TValue value);
    protected abstract TValue ReadValue(NetReader reader);

    protected override void Write(NetWriter writer, ChangePacket<TValue> data)
    {
        writer.Write(data.Type);
        if (data.Type == ChangeType.Clear)
            return;
        
        WriteValue(writer, data.Value);
    }

    protected override void Read(NetReader reader)
    {
        var change = reader.ReadEnum<ChangeType>();

        switch (change)
        {
            case ChangeType.Add:
                AddValue(ReadValue(reader), false);
                break;
            case ChangeType.Remove:
                RemoveValue(ReadValue(reader), false);
                break;
            case ChangeType.Clear:
                Clear(false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}