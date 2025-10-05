using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class IDToHashSyncedDictionary : SyncedDictionary<byte, ulong>
{
    public IDToHashSyncedDictionary(string name) : base(name)
    {
    }

    protected override int? GetSize(Pair<byte, ulong> data)
    {
        return sizeof(byte) + sizeof(ulong);
    }

    protected override void WritePair(NetWriter writer, Pair<byte, ulong> pair)
    {
        writer.Write(pair.Key);
        writer.Write(pair.Value);
    }

    protected override Pair<byte, ulong> ReadPair(NetReader reader)
    {
        var key = reader.ReadByte();
        var value = reader.ReadUInt64();
        return new Pair<byte, ulong>(key, value);
    }
}