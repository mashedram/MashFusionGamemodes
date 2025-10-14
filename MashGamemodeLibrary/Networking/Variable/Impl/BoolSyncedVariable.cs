using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Validation;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class BoolSyncedVariable : SyncedVariable<bool>
{
    public BoolSyncedVariable(string name, bool defaultValue, INetworkRoute? route = null) : base(name, defaultValue,
        route)
    {
    }

    protected override int? GetSize(bool data)
    {
        return sizeof(bool);
    }

    protected override bool Equals(bool a, bool b)
    {
        return a == b;
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