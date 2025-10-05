using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class EnumSyncedVariable<T> : SyncedVariable<T> where T : struct, Enum
{
    public EnumSyncedVariable(string name, T defaultValue) : base(name, defaultValue)
    {
    }

    protected override int? GetSize(T data)
    {
        return sizeof(int);
    }
    
    protected override T ReadValue(NetReader reader)
    {
        return reader.ReadEnum<T>();
    }

    protected override void WriteValue(NetWriter writer, T value)
    {
        writer.Write(value);
    }
}