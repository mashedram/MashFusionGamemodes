using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.Registry;
using MashGamemodeLibrary.Registry.Typed;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class IDToInstanceSyncedDictionary<T> : SyncedDictionary<byte, T> where T : class
{
    public readonly FactoryTypedRegistry<T> Registry = new();
    
    public IDToInstanceSyncedDictionary(string name) : base(name)
    {
        
    }

    protected override int? GetSize(DictionaryEdit<byte, T> data)
    {
        var selfSize = sizeof(byte) + sizeof(ulong);
        if (data.Value is INetSerializable serializable)
            selfSize += serializable.GetSize() ?? 4096;
        return selfSize;
    }

    protected override void WriteKey(NetWriter writer, byte key)
    {
        writer.Write(key);
    }

    protected override byte ReadKey(NetReader reader)
    {
        return reader.ReadByte();
    }

    protected override void WriteValue(NetWriter writer, T value)
    {
        writer.Write(Registry.GetID(value));
        
        if (value is not INetSerializable serializable) return;
        
        serializable.Serialize(writer);
    }

    protected override T ReadValue(NetReader reader, byte key)
    {
        var id = reader.ReadUInt64();
        if (!Registry.TryGet(id, out var instance))
            throw new Exception($"Failed to find an instance of id {id} in the registry for {typeof(T).Name}. Did you register the types?");

        if (instance is INetSerializable serializable) serializable.Serialize(reader);

        return instance;
    }
}