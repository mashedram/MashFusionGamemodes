using Clockhunt.Audio.Effectors.Weather;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Constraints;
using MashGamemodeLibrary.Config.Menu;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Util;

namespace Clockhunt.Config;

class SecondsToMinutesDisplayTransformer : IConfigDisplayTransformer<int, float>
{
    public float ToDisplay(int value)
    {
        return value / 60f;
    }

    public int FromDisplay(float display)
    {
        return (int)(display * 60);
    }
}

public class ClockhuntConfig : IConfig
{
    private readonly AutoSerializer<ClockhuntConfig> _autoSerializer = new();
    
    [ConfigMenuEntry("Hide phase duration")]
    [ConfigRangeConstraint(15, 1200)]
    [ConfigStepSize(15)]
    [ConfigDisplayTransformer(typeof(float), typeof(SecondsToMinutesDisplayTransformer))]
    public int HidePhaseDuration = 150;
    public int HuntPhaseDuration = 1200;
    public int EscapePhaseDuration = 240;

    public int ClocksPerPlayer = 1;
    public int HuntPhaseClockCount = 2;

    public bool IsSpectatingEnabled = true;
    public bool IsEscapePhaseEnabled = true;

    public float DeliveryDistance = 10.0f;

    public WeatherType WeatherType = WeatherType.None;

    public bool TeleportToSpawn = false;
    public bool NightVision = true;
    public float NightVisionBrightness = 1.0f;

    public bool RuntimeSpawnPointsEnabled = false;
    public int RuntimeSpawnCount = 6;

    public bool DevToolsDisabled = true;

    public bool DebugForceSpectate = false;
    public bool DebugSkipNightmare = false;

    public PlayerStats DefaultStats = new()
    {
        Vitality = 1.0f,
        Speed = 1.0f,
        LowerStrength = 1.0f,
        UpperStrength = 1.0f,
        Agility = 1.0f
    };

    public float EscapeDistance = 10.0f;
    public float EscapeDuration = 15.0f;

    public void Serialize(INetSerializer serializer)
    {
        _autoSerializer.Serialize(serializer, this);
    }
}