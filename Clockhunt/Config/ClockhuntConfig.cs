using Clockhunt.Audio.Effectors.Weather;
using Clockhunt.Nightmare;
using LabFusion.Menu.Data;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Constraints;
using MashGamemodeLibrary.Config.Menu;
using MashGamemodeLibrary.networking.Variable.Impl;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Util;

namespace Clockhunt.Config;

class SecondsToMinutesDisplayTransformer : IConfigDisplayTransformer
{
    public object ToDisplay(object value)
    {
        return (int)value / 60f;
    }

    public object FromDisplay(object display)
    {
        return (int)((float)display * 60);
    }
}

public class ClockhuntConfig : AutoSerialized, IConfig, IConfigMenuProvider
{
    [ConfigMenuEntry("Hide phase duration")]
    [ConfigRangeConstraint(15, 1200)]
    [ConfigStepSize(15)]
    [ConfigDisplayTransformer(typeof(float), typeof(SecondsToMinutesDisplayTransformer))]
    public int HidePhaseDuration = 150;
    [ConfigMenuEntry("Hide phase duration")]
    [ConfigRangeConstraint(15, 1200)]
    [ConfigStepSize(15)]
    [ConfigDisplayTransformer(typeof(float), typeof(SecondsToMinutesDisplayTransformer))]
    public int HuntPhaseDuration = 1200;
    [ConfigMenuEntry("Hide phase duration")]
    [ConfigRangeConstraint(15, 1200)]
    [ConfigStepSize(15)]
    [ConfigDisplayTransformer(typeof(float), typeof(SecondsToMinutesDisplayTransformer))]
    public int EscapePhaseDuration = 240;
    
    // TODO: Add to config screen once clock overhaul
    public int ClocksPerPlayer = 1;
    [ConfigMenuEntry("Hunt phase clock count")]
    [ConfigRangeConstraint(1, 10)]
    public int HuntPhaseClockCount = 2;

    [ConfigMenuEntry("Spectating Enabled")]
    public bool IsSpectatingEnabled = true;
    [ConfigMenuEntry("Escape Enabled")]
    public bool IsEscapePhaseEnabled = true;

    public float DeliveryDistance = 10.0f;

    [ConfigMenuEntry("Weather Type")]
    public WeatherType WeatherType = WeatherType.None;

    [ConfigMenuEntry("Teleport to Spawn")]
    public bool TeleportToSpawn = false;
    [ConfigMenuEntry("Nightmare Night Vision")]
    [Synchronise]
    public bool NightVision = true;
    [ConfigMenuEntry("Night Vision Brightness")]
    [Synchronise]
    public float NightVisionBrightness = 1.0f;

    [ConfigMenuEntry("Runtime Spawnpoints")]
    public bool RuntimeSpawnPointsEnabled = false;
    [ConfigMenuEntry("Runtime Spawnpoint Count")]
    [ConfigRangeConstraint(1, 20)]
    public int RuntimeSpawnCount = 6;

    [ConfigMenuEntry("Dev Tools Enabled")]
    [Synchronise]
    public bool DevToolsDisabled = true;
    
    [ConfigMenuEntry("Debug - Force Spectate")]
    public bool DebugForceSpectate = false;
    [ConfigMenuEntry("Debug - Skip Enabled")]
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

    [ConfigMenuEntry("Max Lives")]
    [ConfigRangeConstraint(0, 5)]
    public int MaxLives = 3;

    public void AddExtraFields(GroupElementData root)
    {
        var nightmares = new GroupElementData("Nightmares");
        foreach (var (_, descriptor) in NightmareManager.Registry)
        {
            nightmares.AddElement(new BoolElementData
            {
                Title = descriptor.Name,
                Value = descriptor.IsEnabled,
                OnValueChanged = value => descriptor.IsEnabled = value
            });
        }
        
        root.AddElement(nightmares);
    }
}