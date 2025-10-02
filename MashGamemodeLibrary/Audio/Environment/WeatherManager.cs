using Il2CppSLZ.Marrow.Pool;
using LabFusion.Marrow.Pool;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Execution;
using MashGamemodeLibrary.networking;
using MelonLoader;
using UnityEngine;

namespace MashGamemodeLibrary.Audio.Environment;

class WeatherPacket : INetSerializable
{
    public string[] WeatherTypes = Array.Empty<string>();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref WeatherTypes);
    }
}

public static class WeatherManager
{
    private static readonly RemoteEvent<WeatherPacket> OnWeatherChangeEvent = new(OnWeatherChange);
    
    private static readonly List<Poolee> WeatherEntities = new();
    
    private static void ClearLocalWeather()
    {
        foreach (var entity in WeatherEntities)
        {
            AssetSpawner.Despawn(entity);
        }
        
        WeatherEntities.Clear();
    }
    
    private static void SpawnLocalWeather(string barcode)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(barcode);
        
        LocalAssetSpawner.Register(spawnable);
        
        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, poolee =>
        {
            WeatherEntities.Add(poolee);
        });
    }

    public static void SetWeather(string[] weatherTypes)
    {
        Executor.RunIfHost(() =>
        {
            var packet = new WeatherPacket
            {
                WeatherTypes = weatherTypes,
            };
            
            OnWeatherChangeEvent.Call(packet);
        });
    }

    // Poke it with a stick to wake it the fuck up
    // Need to change this in the future
    // TODO: Make a global system to ensure all remoteevents are registered
    public static void Register()
    {
        MelonLogger.Msg("Registered WeatherManager" + OnWeatherChangeEvent);
    }
    
    // Remote Events
    
    private static void OnWeatherChange(WeatherPacket packet)
    {
        ClearLocalWeather();
        foreach (var barcode in packet.WeatherTypes)
        {
            SpawnLocalWeather(barcode);
        }
    }
}