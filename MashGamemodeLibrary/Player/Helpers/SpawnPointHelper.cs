using LabFusion.Utilities;
using UnityEngine;

namespace MashGamemodeLibrary.Player.Helpers;

public static class SpawnPointHelper
{
    private static GameObject? _spawnPoint;
    
    private static GameObject GetSpawnPoint()
    {
        if (_spawnPoint == null)
        {
            _spawnPoint = new GameObject();
        }
        
        return _spawnPoint;
    }
    
    public static void SetSpawnPoint(Vector3 position)
    {
        var spawnPoint = GetSpawnPoint();
        spawnPoint.transform.SetPositionAndRotation(position, Quaternion.identity);
        FusionPlayer.SetSpawnPoints(spawnPoint.transform);
    }
}