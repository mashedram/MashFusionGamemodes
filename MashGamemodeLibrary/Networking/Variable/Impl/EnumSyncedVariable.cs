using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.networking.Validation;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class EnumSyncedVariable<T> : SyncedVariable<T> where T : struct, Enum
{
    public EnumSyncedVariable(string name, T defaultValue, INetworkRoute? route = null) : base(name, defaultValue, route)
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