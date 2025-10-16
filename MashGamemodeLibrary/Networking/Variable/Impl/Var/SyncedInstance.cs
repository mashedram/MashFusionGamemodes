using LabFusion.Network.Serialization;
using MelonLoader;

namespace MashGamemodeLibrary.networking.Variable.Impl.Var;

public class SyncedInstance<TValue> : SyncedVariable<TValue?> where TValue : class, INetSerializable
{
    private Registry.Registry<TValue> _registry;
    
    public SyncedInstance(string name, Registry.Registry<TValue> registry) : base(name, null)
    {
        _registry = registry;
    }
    
    protected override int? GetSize(TValue? data)
    {
        if (data == null) return 0;
        
        return data.GetSize();
    }
    protected override bool Equals(TValue? a, TValue? b)
    {
        if (a == null) return b == null;

        return a.Equals(b);
    }
    protected override TValue? ReadValue(NetReader reader)
    {
        if (!reader.ReadBoolean())
            return null;

        var id = reader.ReadUInt64();
        var value = _registry.Get(id);

        if (value == null)
        {
            MelonLogger.Error($"No value registered by id {id} in {typeof(TValue).Name}'s registry.");
            return null;
        }
        
        value.Serialize(reader);
        return value;
    }
    protected override void WriteValue(NetWriter writer, TValue? value)
    {
        writer.Write(value != null);
        if (value == null) return;
        
        writer.Write(_registry.GetID(value));
        value.Serialize(writer);
    }
    
}