using Clockhunt.Audio.Effectors.Weather;
using Clockhunt.Nightmare;
using Clockhunt.Nightmare.Config;
using LabFusion.Menu.Data;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Constraints;
using MashGamemodeLibrary.Config.Menu;
using MashGamemodeLibrary.Config.Menu.Attributes;
using MashGamemodeLibrary.Player;
using MashGamemodeLibrary.Util;

namespace Clockhunt.Config;

public enum GameType
{
    Clockhunt,
    HideAndSeek
}

internal class SecondsToMinutesElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<object> setter)
    {
        return new FloatElementData
        {
            Title = entry.Name,
            Increment = 0.25f,
            MaxValue = 10f,
            MinValue = 0.25f,
            Value = Convert.ToSingle(entry.DefaultValue) / 60f,
            OnValueChanged = f => setter(Convert.ToSingle(f) * 60f)
        };
    }
}

public class ClockhuntConfig : AutoSerialized<ClockhuntConfig>, IConfig, IConfigMenuProvider
{
    [ConfigMenuEntry("Game Type")]
    [SerializableField]
    public GameType GameType = GameType.Clockhunt;
    
    [ConfigMenuEntry("Hide phase duration", "Time")]
    [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))]
    [SerializableField]
    public float HidePhaseDuration = 150f;
    
    [ConfigMenuEntry("Hunt phase duration", "Time")]
    [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))]
    [SerializableField]
    public float HuntPhaseDuration = 600f;
    
    [ConfigMenuEntry("Escape phase duration", "Time")]
    [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))]
    [SerializableField]
    public float EscapePhaseDuration = 240f;
    
    // TODO: Add to config screen once clock overhaul
    public int ClocksPerPlayer = 1;
    [ConfigMenuEntry("Hunt phase clock count", "Clocks")]
    [ConfigRangeConstraint(1, 10)]
    public int HuntPhaseClockCount = 2;
    
    [ConfigMenuEntry("Escape Enabled", "Difficulty")]
    public bool IsEscapePhaseEnabled = true;
    
    [ConfigMenuEntry("Teleport to Spawn On Start", "Difficulty")]
    [SerializableField]
    public bool TeleportToSpawn = false;

    [ConfigMenuEntry("Delivery Distance", "Difficulty")]
    public float DeliveryDistance = 10.0f;
    
    [ConfigMenuEntry("Max Respawns", "Difficulty")]
    [ConfigRangeConstraint(0, 5)]
    public int MaxRespawns = 2;
    [ConfigMenuEntry("Nightmare Health Mult", "Difficulty")]
    [ConfigRangeConstraint(1f, 5f)]
    public float NightmareHealthMult = 1f;
    
    
    [ConfigMenuEntry("Weather Type", "Environment")]
    public WeatherType WeatherType = WeatherType.None;

    [ConfigMenuEntry("Music", "Environment")] 
    [SerializableField]
    public bool MusicEnabled = true;
    
    [ConfigMenuEntry("Nightmare Night Vision", "Environment")]
    [SerializableField]
    public bool NightVision = true;
    [ConfigMenuEntry("Night Vision Brightness", "Environment")]
    [SerializableField]
    public float NightVisionBrightness = 1.0f;

    [ConfigMenuEntry("Runtime Spawnpoints", "EXPERIMENTAL")]
    public bool RuntimeSpawnPointsEnabled = false;
    [ConfigMenuEntry("Runtime Spawnpoint Count", "EXPERIMENTAL")]
    [ConfigRangeConstraint(1, 20)]
    public int RuntimeSpawnCount = 6;

    [ConfigMenuEntry("Dev Tools Enabled", "EXPERIMENTAL")]
    [SerializableField]
    public bool DevToolsDisabled = true;
    
    [ConfigMenuEntry("Debug - Force Spectate", "EXPERIMENTAL")]
    public bool DebugSkipSpectate = false;
    [ConfigMenuEntry("Debug - Skip Nightmare", "EXPERIMENTAL")]
    public bool DebugSkipNightmare = false;

    public PlayerStats DefaultStats = new()
    {
        Vitality = 0.75f,
        Speed = 1.1f,
        LowerStrength = 1.2f,
        UpperStrength = 1.2f,
        Agility = 1.2f
    };

    public float EscapeDistance = 10.0f;
    
    public void AddExtraFields(GroupElementData root)
    {
        foreach (var (_, descriptor) in NightmareManager.Descriptors)
        {
            var group = new GroupElementData(descriptor.Name);
            group.AddElement(new BoolElementData
            {
                Title = "Enabled",
                Value = descriptor.Enabled,
                OnValueChanged = value => descriptor.Enabled = value
            });

            descriptor.GetConfig<NightmareConfig>().AttachToGroup(group);
            root.AddElement(group);
        }
    }
}