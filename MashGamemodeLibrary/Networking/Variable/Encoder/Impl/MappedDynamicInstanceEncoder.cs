using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Registry.Typed;
using MelonLoader;

namespace MashGamemodeLibrary.networking.Variable.Encoder.Impl;

public class MappedDynamicInstanceEncoder<TInternal, TValue> : IRefEncoder<TValue> where TValue : class where TInternal : class
{
    public delegate TValue MapToValueDelegate(TInternal @internal);
    public delegate ulong MapToKeyDelegate(ITypedRegistry<TInternal> registry, TValue value);
    
    private readonly ITypedRegistry<TInternal> _typedRegistry;
    private readonly MapToValueDelegate _mapToValue;
    private readonly MapToKeyDelegate _mapToKey;

    public MappedDynamicInstanceEncoder(ITypedRegistry<TInternal> typedRegistry, MapToValueDelegate mapToValue, MapToKeyDelegate mapToKey)
    {
        _typedRegistry = typedRegistry;
        _mapToValue = mapToValue;
        _mapToKey = mapToKey;
    }

    public int GetSize(TValue value)
    {
        var selfSize = sizeof(ulong);
        if (value is INetSerializable serializable)
            selfSize += serializable.GetSize() ?? 4096;
        return selfSize;
    }

    public TValue Read(NetReader reader)
    {
        var id = reader.ReadUInt64();
        if (!_typedRegistry.TryGet(id, out var value))
            throw new Exception($"Failed to fetch: {id} from registry of type: {typeof(TValue).Name}");

        switch (value)
        {
            case null:
                MelonLogger.Error($"No value registered by id {id} in {typeof(TValue).Name}'s registry.");
                return null!;

            case INetSerializable serializable:
                serializable.Serialize(reader);
                break;
        }

        return _mapToValue(value);
    }

    public void Write(NetWriter writer, TValue value)
    {
        writer.Write(_mapToKey(_typedRegistry, value));

        if (value is not INetSerializable serializable) return;

        serializable.Serialize(writer);
    }

    public void Serialize(INetSerializer serializer, TValue value)
    {
        var id = serializer.IsReader ? 0 : _mapToKey(_typedRegistry, value);
        serializer.SerializeValue(ref id);
        
        if (value is INetSerializable value2)
            value2.Serialize(serializer);
    }
}