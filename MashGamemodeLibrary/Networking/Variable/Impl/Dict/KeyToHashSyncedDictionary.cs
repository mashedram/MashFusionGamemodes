using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.Registry;

namespace MashGamemodeLibrary.Networking.Variable.Impl.Dict;

public class KeyToInstanceSyncedDictionary<TKey, TValue> : SyncedDictionary<TKey, TValue> 
    where TKey : INetSerializable, new()
    where TValue : class
{
    public readonly FactoryRegistry<TValue> Registry = new();
    
    public KeyToInstanceSyncedDictionary(string name) : base(name)
    {
        
    }

    protected override int? GetSize(DictionaryEdit<TKey, TValue> data)
    {
        return data.Key.GetSize() + sizeof(ulong);
    }

    protected override void WriteKey(NetWriter writer, TKey key)
    {
        key.Serialize(writer);
    }

    protected override TKey ReadKey(NetReader reader)
    {
        var key = new TKey();
        key.Serialize(reader);
        return key;
    }

    protected override void WriteValue(NetWriter writer, TValue value)
    {
        writer.Write(Registry.GetID(value));
        
        if (value is not INetSerializable serializable) return;
        
        serializable.Serialize(writer);
    }

    protected override TValue ReadValue(NetReader reader, TKey key)
    {
        var id = reader.ReadUInt64();
        if (!Registry.TryGet(id, out var instance))
            throw new Exception($"Failed to find an instance of id {id} in the registry for {typeof(TValue).Name}. Did you register the types?");

        if (instance is INetSerializable serializable) serializable.Serialize(reader);

        return instance;
    }
}