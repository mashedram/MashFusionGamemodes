using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class FloatSyncedVariable : SyncedVariable<float>
{
    public FloatSyncedVariable(string name, float defaultValue) : base(name, defaultValue)
    {
    }

    protected override int? GetSize(float data)
    {
        return sizeof(float);
    }

    protected override float ReadValue(NetReader reader)
    {
        return reader.ReadSingle();
    }

    protected override void WriteValue(NetWriter writer, float value)
    {
        writer.Write(value);
    }
}