using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using LabFusion.Marrow.Pool;
using UnityEngine;

namespace MashGamemodeLibrary.Environment.Effector.Weather;

public static class LocalWeatherManager
{
    private static readonly List<Poolee> WeatherEntities = new();

    public static void ClearLocalWeather()
    {
        foreach (var entity in WeatherEntities.OfType<Poolee>())
            AssetSpawner.Despawn(entity);
        
        WeatherEntities.Clear();
    }

    public static void SpawnLocalWeather(string barcode)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(barcode);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity,
            poolee =>
            {
                var entity = poolee.GetComponent<MarrowEntity>();
                if (entity != null)
                    entity.PreventDisableOnCull();
                
                WeatherEntities.Add(poolee); 
            });
    }
}