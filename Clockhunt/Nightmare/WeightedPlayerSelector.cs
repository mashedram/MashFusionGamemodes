using LabFusion.Entities;
using LabFusion.Player;
using UnityEngine;

namespace Clockhunt.Nightmare;

public class WeightedPlayerSelector
{
    private int? _lastSelectedPlayerID;
    private int _sameSelectionCount = 0;
    private Dictionary<byte, int> _weights = new();
    
    private int GetTotalWeight()
    {
        return _weights.Values.Sum();
    }

    private void RebuildWeights()
    {
        var playerIDs = PlayerIDManager.PlayerIDs.Select(e => e.SmallID).ToList();

        foreach (var playerID in _weights.Keys.Except(playerIDs))
        {
            _weights.Remove(playerID);
        }

        foreach (var playerID in playerIDs.Except(_weights.Keys))
        {
            _weights[playerID] = 1;
        }
    }
    
    private byte SelectPlayer()
    {
        var totalWeight = GetTotalWeight();
        if (totalWeight <= 0)
            return byte.MaxValue;
        
        var randomValue = UnityEngine.Random.Range(0, totalWeight);
        foreach (var kvp in _weights)
        {
            if (randomValue < kvp.Value)
                return kvp.Key;
            randomValue -= kvp.Value;
        }
        
        return byte.MaxValue;
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