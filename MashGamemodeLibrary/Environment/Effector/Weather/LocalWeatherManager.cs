using Il2CppSLZ.Marrow.Pool;
using LabFusion.Marrow.Pool;
using MashGamemodeLibrary.Audio.Environment;
using MashGamemodeLibrary.Execution;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Environment.Effector.Weather;

public static class LocalWeatherManager
{
    private static readonly List<Poolee> WeatherEntities = new();
    
    public static void ClearLocalWeather()
    {
        foreach (var entity in WeatherEntities)
        {
            AssetSpawner.Despawn(entity);
        }
        
        WeatherEntities.Clear();
    }
    
    public static void SpawnLocalWeather(string barcode)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(barcode);
        
        LocalAssetSpawner.Register(spawnable);
        
        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, poolee =>
        {
            WeatherEntities.Add(poolee);
        });
    }
}