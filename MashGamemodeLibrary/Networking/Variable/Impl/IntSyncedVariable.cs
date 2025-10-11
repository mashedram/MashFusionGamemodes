using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Control;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class IntSyncedVariable : SyncedVariable<int>
{
    public IntSyncedVariable(string name, int defaultValue) : base(name, defaultValue)
    {
    }

    protected override bool Equals(int a, int b)
    {
        return a == b;
    }

    protected override int? GetSize(int data)
    {
        return sizeof(int);
    }

    protected override int ReadValue(NetReader reader)
    {
        return reader.ReadInt32();
    }

    protected override void WriteValue(NetWriter writer, int value)
    {
        writer.Write(value);
    }
}