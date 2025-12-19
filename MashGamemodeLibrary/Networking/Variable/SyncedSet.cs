using System.Collections;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable.Encoder;

namespace MashGamemodeLibrary.networking.Variable;

public enum ChangeType
{
    Add,
    Remove,
    Clear
}

public record ChangePacket<T>(ChangeType Type, T Value);

public class SyncedSet<TValue> : GenericRemoteEvent<ChangePacket<TValue>>, IEnumerable<TValue>, ICatchup, IResettable
    where TValue : notnull
{
    public delegate void ValueChangedHandler(TValue value);
    public delegate void ValueRemovedHandler(TValue oldValue);

    private readonly IEncoder<TValue> _encoder;
    private readonly HashSet<TValue> _set = new();

    public SyncedSet(string name, IEncoder<TValue> encoder) : this(name, encoder, CommonNetworkRoutes.HostToAll)
    {
    }

    public SyncedSet(string name, IEncoder<TValue> encoder, INetworkRoute route) : base(name, route)
    {
        _encoder = encoder;
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
        Relay(new ChangePacket<TValue>(ChangeType.Add, value));
    }

    private void RelayRemove(TValue value)
    {
        Relay(new ChangePacket<TValue>(ChangeType.Remove, value));
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
            Relay(new ChangePacket<TValue>(ChangeType.Clear, default!));
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

    protected override int? GetSize(ChangePacket<TValue> data)
    {
        if (data.Type == ChangeType.Clear)
            return sizeof(int);

        return sizeof(int) + _encoder.GetSize(data.Value);
    }

    protected override void Write(NetWriter writer, ChangePacket<TValue> data)
    {
        writer.Write(data.Type);
        if (data.Type == ChangeType.Clear)
            return;

        _encoder.Write(writer, data.Value);
    }

    protected override void Read(byte smallId, NetReader reader)
    {
        var change = reader.ReadEnum<ChangeType>();

        switch (change)
        {
            case ChangeType.Add:
                AddValue(_encoder.Read(reader), false);
                break;
            case ChangeType.Remove:
                RemoveValue(_encoder.Read(reader), false);
                break;
            case ChangeType.Clear:
                Clear(false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}