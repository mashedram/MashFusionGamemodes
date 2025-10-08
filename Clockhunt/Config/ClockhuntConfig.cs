using Clockhunt.Audio.Effectors.Weather;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Player;

namespace Clockhunt.Config;

public static class ClockhuntConfig
{
    public static int HidePhaseDuration = 150;
    public static int HuntPhaseDuration = 1200;
    public static int EscapePhaseDuration = 240;
    
    public static int ClocksPerPlayer = 1;
    public static int HuntPhaseClockCount = 2;
    
    public static bool IsSpectatingEnabled = true;
    public static bool IsEscapePhaseEnabled = true;
    
    public static float DeliveryDistance = 10.0f;
    
    public static EnumSyncedVariable<WeatherType> WeatherType = new("weather_type", Audio.Effectors.Weather.WeatherType.None);
    public static BoolSyncedVariable TeleportToSpawn = new("teleport_to_spawn", false);
    public static BoolSyncedVariable NightVision = new("night_vision", true);
    public static FloatSyncedVariable NightVisionBrightness = new("night_vision_brightness", 1.0f);
    
    public static bool DevToolsDisabled = true;
    
    public static PlayerStats DefaultStats = new PlayerStats
    {
        Vitality = 1.0f,
        Speed = 1.0f,
        LowerStrength = 1.0f,
        UpperStrength = 1.0f,
        Agility = 1.0f
    };

    public static float EscapeDistance = 10.0f;
    public static float EscapeDuration = 15.0f;
}