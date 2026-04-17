using System.Text.Json.Serialization;
using LabFusion.Menu.Data;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Menu;
using MashGamemodeLibrary.Config.Menu.Attributes;
using MashGamemodeLibrary.Util;
using TheHunt.Audio.Effectors.Weather;

namespace TheHunt.Config;

internal class SecondsToMinutesElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter)
    {
        return new FloatElementData
        {
            Title = entry.Name,
            Increment = 0.25f,
            MaxValue = 10f,
            MinValue = 0.25f,
            Value = Convert.ToSingle(entry.Value) / 60f,
            OnValueChanged = f => setter(entry, Convert.ToSingle(f) * 60f)
        };
    }
}

public class TheHuntConfig : IConfig
{
    [ConfigMenuEntry("Hide Phase Duration", "Time")] [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))] 
    [JsonInclude]
    public float HideDuration = 90f;
    
    [ConfigMenuEntry("Hide Phase Duration", "Time")] [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))] 
    [JsonInclude]
    public float HuntDuration = 360f;
    
    [ConfigMenuEntry("Finally Enabled", "Time")]
    [JsonInclude]
    public bool FinallyEnabled = true;
    
    [ConfigMenuEntry("Hide Phase Duration", "Time")] [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))]
    [JsonInclude]
    public float FinallyDuration = 60f;

    [ConfigMenuEntry("Lock Nightmare on Start", "Health")] [JsonInclude]
    public bool LockNightmare = true;

    [ConfigMenuEntry("Blind when locked", "Health")] [JsonInclude]
    public bool BlindNightmare = true;
    
    [ConfigMenuEntry("Balance Avatar Health", "Health")] [JsonInclude]
    public bool BalanceStats = true;
    
    [ConfigMenuEntry("Weather Type", "Environment")]
    public WeatherType WeatherType = WeatherType.None;

    [ConfigMenuEntry("Music", "Environment")] [SerializableField]
    public bool MusicEnabled = true;

    [ConfigMenuEntry("Nightmare Night Vision", "Environment")] [SerializableField]
    public bool NightVision = true;

    [ConfigMenuEntry("Night Vision Brightness", "Environment")] [SerializableField]
    public float NightVisionBrightness = 1.0f;

    [ConfigMenuEntry("Dev Tools Disabled", "Utility")] 
    [JsonInclude]
    public bool DevToolsDisabled = true;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref HideDuration);
        serializer.SerializeValue(ref HuntDuration);
        serializer.SerializeValue(ref FinallyDuration);
        serializer.SerializeValue(ref LockNightmare);
        serializer.SerializeValue(ref BlindNightmare);
        serializer.SerializeValue(ref BalanceStats);
        serializer.SerializeValue(ref DevToolsDisabled);
    }
    
    public object Clone()
    {
        return new TheHuntConfig()
        {
            HideDuration = HideDuration,
            HuntDuration = HuntDuration,
            FinallyDuration = FinallyDuration,
            LockNightmare = LockNightmare,
            BlindNightmare = BlindNightmare,
            BalanceStats = BalanceStats,
            DevToolsDisabled = DevToolsDisabled
        };
    }
}