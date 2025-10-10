using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Control;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class EnumSyncedVariable<T> : SyncedVariable<T> where T : struct, Enum
{
    public EnumSyncedVariable(string name, T defaultValue, CatchupMoment moment = CatchupMoment.Join) : base(name, defaultValue, moment)
    {
    }

    protected override int? GetSize(T data)
    {
        return sizeof(int);
    }

    protected override bool Equals(T a, T b)
    {
        return a.Equals(b);
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