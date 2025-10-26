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

internal class SecondsToMinutesElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(string name, object value, Action<object> setter)
    {
        return new FloatElementData
        {
            Title = name,
            Increment = 0.25f,
            MaxValue = 10f,
            MinValue = 0.25f,
            Value = Convert.ToSingle(value) / 60f,
            OnValueChanged = f => setter(Convert.ToSingle(f) * 60f)
        };
    }
}

public class ClockhuntConfig : AutoSerialized<ClockhuntConfig>, IConfig, IConfigMenuProvider
{
    [ConfigMenuEntry("Hide phase duration")]
    [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))]
    public float HidePhaseDuration = 150f;
    
    [ConfigMenuEntry("Hide phase duration")]
    [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))]
    public float HuntPhaseDuration = 1200f;
    
    [ConfigMenuEntry("Hide phase duration")]
    [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))]
    public float EscapePhaseDuration = 240f;
    
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
    [SerializableField]
    public bool NightVision = true;
    [ConfigMenuEntry("Night Vision Brightness")]
    [SerializableField]
    public float NightVisionBrightness = 1.0f;

    [ConfigMenuEntry("Runtime Spawnpoints")]
    public bool RuntimeSpawnPointsEnabled = false;
    [ConfigMenuEntry("Runtime Spawnpoint Count")]
    [ConfigRangeConstraint(1, 20)]
    public int RuntimeSpawnCount = 6;

    [ConfigMenuEntry("Dev Tools Enabled")]
    [SerializableField]
    public bool DevToolsDisabled = true;
    
    [ConfigMenuEntry("Debug - Force Spectate")]
    public bool DebugSkipSpectate = false;
    [ConfigMenuEntry("Debug - Skip Nightmare")]
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

    [ConfigMenuEntry("Max Respawns")]
    [ConfigRangeConstraint(0, 5)]
    public int MaxRespawns = 2;

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