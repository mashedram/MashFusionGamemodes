using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Validation;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class ByteSyncedSet : SyncedSet<byte>
{
    public ByteSyncedSet(string name) : base(name)
    {
    }

    protected override int GetValueSize(byte data)
    {
        return sizeof(byte);
    }

    protected override void WriteValue(NetWriter writer, byte value)
    {
        writer.Write(value);
    }

    protected override byte ReadValue(NetReader reader)
    {
        return reader.ReadByte();
    }
}