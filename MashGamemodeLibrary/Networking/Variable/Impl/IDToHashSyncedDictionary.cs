using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Control;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class IDToHashSyncedDictionary : SyncedDictionary<byte, ulong>
{
    public IDToHashSyncedDictionary(string name, CatchupMoment moment) : base(name, moment)
    {
    }

    protected override int? GetSize(Pair<byte, ulong> data)
    {
        return sizeof(byte) + sizeof(ulong);
    }


    protected override void WriteKey(NetWriter writer, byte key)
    {
        writer.Write(key);
    }

    protected override byte ReadKey(NetReader reader)
    {
        return reader.ReadByte();
    }

    protected override void WriteValue(NetWriter writer, ulong value)
    {
        writer.Write(value);
    }

    protected override ulong ReadValue(NetReader reader)
    {
        return reader.ReadUInt64();
    }
}