using System.Collections;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;

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

public abstract class SyncedSet<TValue> : GenericRemoteEvent<ChangePacket<TValue>>, IEnumerable<TValue>, ICatchup,
    IResettable
{
    public delegate void ValueChangedHandler(TValue value);

    public delegate void ValueRemovedHandler(TValue oldValue);


    private readonly HashSet<TValue> _set = new();

    protected SyncedSet(string name, INetworkRoute route) : base(name, route)
    {
    }

    public int Count => _set.Count;

    public void OnCatchup(PlayerID playerId)
    {
        foreach (var value in _set) RelayAdd(value);
    }

    public IEnumerator<TValue> GetEnumerator()
    {
        return _set.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Reset()
    {
        Clear(false);
    }

    public event ValueChangedHandler? OnValueAdded;
    public event ValueRemovedHandler? OnValueRemoved;

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

    private void OnServerChanged()
    {
        _set.Clear();
    }

    // Private methods

    private void AddValue(TValue value, bool sendUpdate)
    {
        if (!_set.Add(value)) return;

        OnValueAdded?.Invoke(value);

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
        var removedEntries = _set.ToList();
        _set.Clear();
        removedEntries.ForEach(value => OnValueRemoved?.Invoke(value));

        if (sendUpdate)
            Relay(new ChangePacket<TValue>
            {
                Type = ChangeType.Clear,
                Value = default!
            });
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

    // Abstract methods for serialization

    protected abstract int GetValueSize(TValue value);
    protected abstract void WriteValue(NetWriter writer, TValue value);
    protected abstract TValue ReadValue(NetReader reader);

    protected override int? GetSize(ChangePacket<TValue> data)
    {
        if (data.Type == ChangeType.Clear)
            return sizeof(int);

        return sizeof(int) + GetValueSize(data.Value);
    }

    protected override void Write(NetWriter writer, ChangePacket<TValue> data)
    {
        writer.Write(data.Type);
        if (data.Type == ChangeType.Clear)
            return;

        WriteValue(writer, data.Value);
    }

    protected override void Read(byte playerId, NetReader reader)
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