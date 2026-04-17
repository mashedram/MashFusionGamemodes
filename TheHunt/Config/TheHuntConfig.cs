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
            MaxValue = 30f,
            MinValue = 0.25f,
            Value = Convert.ToSingle(entry.Value) / 60f,
            OnValueChanged = f => setter(entry, Convert.ToSingle(f) * 60f)
        };
    }
}

internal class MinuteElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter)
    {
        return new IntElementData
        {
            Title = entry.Name,
            Increment = 1,
            MaxValue = 30,
            MinValue = 1,
            Value = Convert.ToInt32(entry.Value) / 60,
            OnValueChanged = f => setter(entry, Convert.ToInt32(f) * 60)
        };
    }
}

public class TheHuntConfig : IConfig
{
    [ConfigMenuEntry("Hide Phase Duration", "Time")] [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))] 
    [JsonInclude]
    public float HideDuration = 90f;
    
    [ConfigMenuEntry("Hunt Phase Duration", "Time")] 
    [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))] 
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

    [ConfigMenuEntry("Music", "Environment")]
    public bool MusicEnabled = true;

    [ConfigMenuEntry("Nightmare Night Vision", "Environment")]
    public bool NightVision = true;

    [ConfigMenuEntry("Night Vision Brightness", "Environment")]
    [ConfigRangeConstraint(0.2f, 3f)]
    [ConfigStepSize(0.2f)]
    public float NightVisionBrightness = 1.0f;
    
    [ConfigMenuEntry("Lock Hider Avatars", "Avatars")]
    [JsonInclude]
    public bool LockHiderAvatars = false;
    
    [ConfigMenuEntry("Set Nightmare Avatars", "Avatars")]
    [JsonInclude]
    public bool SetNightmareAvatars = false;

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
        serializer.SerializeValue(ref WeatherType);
        serializer.SerializeValue(ref MusicEnabled);
        serializer.SerializeValue(ref NightVision);
        serializer.SerializeValue(ref NightVisionBrightness);
        serializer.SerializeValue(ref LockHiderAvatars);
        serializer.SerializeValue(ref SetNightmareAvatars);
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
            WeatherType = WeatherType,
            MusicEnabled = MusicEnabled,
            NightVision = NightVision,
            NightVisionBrightness = NightVisionBrightness,
            LockHiderAvatars = LockHiderAvatars,
            SetNightmareAvatars = SetNightmareAvatars,
            BalanceStats = BalanceStats,
            DevToolsDisabled = DevToolsDisabled
        };
    }
}