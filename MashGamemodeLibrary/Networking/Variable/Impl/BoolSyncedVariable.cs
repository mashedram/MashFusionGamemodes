using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Control;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class BoolSyncedVariable : SyncedVariable<bool>
{
    public BoolSyncedVariable(string name, bool defaultValue, CatchupMoment moment = CatchupMoment.Join) : base(name, defaultValue, moment)
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