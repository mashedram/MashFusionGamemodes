using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable;

public class Pair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    public Pair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}

public abstract class SyncedDictionary<TKey, TValue> : GenericRemoteEvent<Pair<TKey, TValue>> where TKey : notnull
{
    public SyncedDictionary(string name) : base(name)
    {
    }
    
    protected abstract void WritePair(NetWriter writer, Pair<TKey, TValue> pair);
    protected abstract Pair<TKey, TValue> ReadPair(NetReader reader);

    protected override void Write(NetWriter writer, Pair<TKey, TValue> data)
    {
        WritePair(writer, data);
    }

    protected override void Read(NetReader reader)
    {
        var pair = ReadPair(reader);
    }
}