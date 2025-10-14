using LabFusion.Entities;
using LabFusion.Player;
using Random = UnityEngine.Random;

namespace Clockhunt.Nightmare;

public class WeightedPlayerSelector
{
    private readonly Dictionary<byte, int> _weights = new();
    private int? _lastSelectedPlayerID;
    private int _sameSelectionCount;

    private void RebuildWeights()
    {
        var playerIDs = PlayerIDManager.PlayerIDs.Select(e => e.SmallID).ToList();

        foreach (var playerID in _weights.Keys.Except(playerIDs)) _weights.Remove(playerID);

        foreach (var playerID in playerIDs.Except(_weights.Keys)) _weights[playerID] = 1;
    }

    private byte SelectPlayer()
    {
        const int baseWeight = 128;
        var inversedWeights = new Dictionary<byte, int>();

        foreach (var (id, weight) in _weights)
        {
            if (!NetworkPlayerManager.TryGetPlayer(id, out var player)) continue;

            if (!player.HasRig)
                continue;

            var newWeight = Math.Max(1, baseWeight - weight);
            inversedWeights[id] = newWeight;
        }

        var totalWeight = inversedWeights.Values.Sum();
        if (totalWeight <= 0)
            return 0;

        var randomValue = Random.Range(0, totalWeight);
        foreach (var kvp in inversedWeights)
        {
            if (randomValue < kvp.Value)
                return kvp.Key;
            randomValue -= kvp.Value;
        }

        // 0 Is the host ID
        return 0;
    }

    private int GetNewWeight(byte playerID, int currentWeight)
    {
        if (_lastSelectedPlayerID != playerID)
        {
            _sameSelectionCount = 1;
            _lastSelectedPlayerID = playerID;
            return currentWeight + 2;
        }

        _sameSelectionCount++;
        _lastSelectedPlayerID = playerID;
        return currentWeight + (int)Math.Pow(2, _sameSelectionCount);
    }

    public byte GetRandomPlayerID()
    {
        RebuildWeights();

        var playerID = SelectPlayer();
        _weights[playerID] = GetNewWeight(playerID, _weights[playerID]);

        return playerID;
    }
}