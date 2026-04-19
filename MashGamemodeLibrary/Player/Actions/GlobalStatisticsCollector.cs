using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.Networking.Remote;
using MashGamemodeLibrary.networking.Validation;
using MashGamemodeLibrary.networking.Variable;
using MashGamemodeLibrary.networking.Variable.Encoder.Impl;
using MashGamemodeLibrary.Networking.Variable.Encoder.Util;
using MashGamemodeLibrary.Registry.Keyed;
using MashGamemodeLibrary.Registry.Typed;
using MashGamemodeLibrary.Util;

namespace MashGamemodeLibrary.Player.Actions;

public class PlayerStatistics : INetSerializable
{
    public byte PlayerID;
    private Dictionary<ulong, int> Statistics { get; } = new();

    public PlayerStatistics()
    {
        PlayerID = 0;
    }

    public PlayerStatistics(byte playerID)
    {
        PlayerID = playerID;
    }

    public void SetStatistic(ulong hash, int value)
    {
        Statistics[hash] = value;
    }

    public int GetValue(Enum key)
    {
        if (!GlobalStatisticsCollector.StatisticKeyIds.TryGet(key, out var hash))
        {
            InternalLogger.Debug($"Getting statistic for {key} with hash {hash} failed, key not registered");
            return 0;
        }

        return Statistics.GetValueOrDefault(hash, 0);

    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PlayerID);

        if (serializer.IsReader)
        {
            var reader = (NetReader)serializer;
            var size = reader.ReadInt32();
            Statistics.Clear();
            for (var i = 0; i < size; i++)
            {
                var hash = reader.ReadUInt64();
                var value = reader.ReadInt32();
                Statistics[hash] = value;
            }
            return;
        }

        var writer = (NetWriter)serializer;
        writer.Write(Statistics.Count);
        foreach (var kvp in Statistics)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }
    }
}

internal class StatisticChangePacket : INetSerializable, IKnownSenderPacket
{
    public ulong KeyHash;
    public int Value;
    public byte SenderSmallId { get; set; }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref KeyHash);
        serializer.SerializeValue(ref Value);
    }
}

[RequireStaticConstructor]
public static class GlobalStatisticsCollector
{
    internal static readonly KeyedRegistry<Enum, ulong> StatisticKeyIds = new();
    private static readonly KeyedRegistry<ulong, Enum> StatisticKeys = new();

    private static readonly SyncedDictionary<byte, PlayerStatistics> PlayerStatistics = new("syncedStatistics", new ByteEncoder(),
        new InstanceEncoder<PlayerStatistics>(), CommonNetworkRoutes.HostToAll);

    private static readonly RemoteEvent<StatisticChangePacket> StatisticChangeEvent =
        new("statisticChange", OnStatisticChangeEvent, CommonNetworkRoutes.AllToHost);

    public static IEnumerable<PlayerStatistics> Statistics =>
        PlayerIDManager.PlayerIDs.Select(playerID => PlayerStatistics.GetValueOrCreate(playerID.SmallID, () => new PlayerStatistics(playerID.SmallID)));

    internal static void RegisterStatisticKey<T>(T key) where T : Enum
    {
        var id = key.GetStableHash();
        StatisticKeyIds.Register(key, id);
        StatisticKeys.Register(id, key);
    }

    internal static void SyncChangeToHost<T>(T key, int value) where T : Enum
    {
        StatisticChangeEvent.CallFor(PlayerIDManager.GetHostID(), new StatisticChangePacket
        {
            KeyHash = StatisticKeyIds.Get(key),
            Value = value
        });
    }

    internal static void Clear()
    {
        Executor.RunIfHost(() =>
        {
            PlayerStatistics.Clear();
        });
    }

    private static void OnStatisticChangeEvent(StatisticChangePacket packet)
    {
        var holder = PlayerStatistics.GetValueOrCreate(packet.SenderSmallId, () => new PlayerStatistics(packet.SenderSmallId));
        holder.SetStatistic(packet.KeyHash, packet.Value);

        PlayerStatistics.Sync(packet.SenderSmallId);
    }
}