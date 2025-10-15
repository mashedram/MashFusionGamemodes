using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Validation;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class HashSyncedVariable : SyncedVariable<ulong?>
{
    public HashSyncedVariable(string name, ulong? defaultValue) : base(name, defaultValue)
    {
    }

    protected override int? GetSize(ulong? data)
    {
        return sizeof(bool) + sizeof(ulong);
    }

    protected override bool Equals(ulong? a, ulong? b)
    {
        return a.Equals(b);
    }

    protected override ulong? ReadValue(NetReader reader)
    {
        if (!reader.ReadBoolean()) return null;

        return reader.ReadUInt64();
    }

    protected override void WriteValue(NetWriter writer, ulong? value)
    {
        writer.Write(value.HasValue);
        
        if (!value.HasValue) return;
        
        writer.Write(value.Value);
    }
}