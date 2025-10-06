using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class BoolSyncedVariable : SyncedVariable<bool>
{
    public BoolSyncedVariable(string name, bool defaultValue) : base(name, defaultValue)
    {
    }

    protected override int? GetSize(bool data)
    {
        return sizeof(bool);
    }

    protected override bool ReadValue(NetReader reader)
    {
        return reader.ReadBoolean();
    }

    protected override void WriteValue(NetWriter writer, bool value)
    {
        writer.Write(value);
    }
}