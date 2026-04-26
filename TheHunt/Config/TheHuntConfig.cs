using System.Text.Json.Serialization;
using LabFusion.Menu.Data;
using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Config;
using MashGamemodeLibrary.Config.Menu;
using MashGamemodeLibrary.Config.Menu.Attributes;
using MashGamemodeLibrary.Loadout;
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

internal class TimeAdditionElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter)
    {
        return new FloatElementData
        {
            Title = entry.Name,
            Increment = 0.5f,
            MaxValue = 3f,
            MinValue = 0f,
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
            Value = (int)(Convert.ToSingle(entry.Value) / 60f),
            OnValueChanged = f => setter(entry, (float)(Convert.ToInt32(f) * 60))
        };
    }
}

internal class BarcodeElement : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter)
    {
        return new SpawnableElementData
        {
            Title = entry.Name,
            OnSetSpawnable = barcode =>
            {
                setter.Invoke(entry, barcode._id);
            }
        };
    }
}

internal class BarcodeListElement : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter)
    {
        var group = new GroupElementData(entry.Name);

        var add = new SpawnableElementData
        {
            Title = "Add Held Item",
            OnSetSpawnable = barcode =>
            {
                var list = (List<string>)entry.Value;

                var id = barcode._id;
                if (list.Contains(id))
                    return;

                list.Add(id);
                setter.Invoke(entry, list);
            }
        };

        var clear = new FunctionElementData
        {
            Title = "Clear",
            OnPressed = () =>
            {
                setter.Invoke(entry, new List<string>());
            }
        };

        group.AddElement(add);
        group.AddElement(clear);

        return group;
    }
}


public class TheHuntConfig : IConfig
{
    [ConfigMenuEntry("Hide Phase Duration", "Time")] [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))] 
    [JsonInclude]
    public float HideDuration = 90f;
    
    [ConfigMenuEntry("Hunt Phase Duration", "Time")] 
    [ConfigElementProvider(typeof(MinuteElementProvider))] 
    [JsonInclude]
    public float HuntDuration = 180f;
    
    [ConfigMenuEntry("Finally Always Plays", "Time")]
    [JsonInclude]
    public bool FinallyAlwaysPlays = true;
    
    [ConfigMenuEntry("Finally Phase Duration", "Time")] [ConfigElementProvider(typeof(SecondsToMinutesElementProvider))]
    [JsonInclude]
    public float FinallyDuration = 60f;
    
    [ConfigMenuEntry("Time Gain on Kill", "Time")]
    [ConfigElementProvider(typeof(TimeAdditionElementProvider))]
    [JsonInclude]
    public float TimeGainOnKill = 60f;

    [ConfigMenuEntry("Lock Nightmare on Start", "Health")] [JsonInclude]
    public bool LockNightmare = true;

    [ConfigMenuEntry("Blind when locked", "Health")] [JsonInclude]
    public bool BlindNightmare = true;
    
    [ConfigMenuEntry("Balance Avatar Health", "Health")] [JsonInclude]
    public bool BalanceStats = true;
    
    [ConfigMenuEntry("Slow nightmare on damage", "Balancing")] 
    [JsonInclude]
    public bool SlowNightmareOnDamage = true;
    [ConfigMenuEntry("Drop nightmare on max damage", "Balancing")] 
    [JsonInclude]
    public bool DropPlayer = true;
    
    [ConfigMenuEntry("Limit Player Magazines", "Balancing")] [JsonInclude]
    public bool LimitMags = true;
    [ConfigMenuEntry("Max ammunition", "Balancing")] 
    [ConfigRangeConstraint(10, 60)]
    [ConfigStepSize(10)]
    [JsonInclude]
    public int MagazineCapacity = 20;
    
    [ConfigMenuEntry("Weather Type", "Environment")]
    public WeatherType WeatherType = WeatherType.None;

    [ConfigMenuEntry("Nightmare Night Vision", "Environment")]
    public bool NightVision = true;
    
    [ConfigMenuEntry("Spectator Night Vision", "Environment")]
    public bool SpectatorNightVision = true;

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
    
    [ConfigMenuEntry("Nightmare Speed", "Speed")]
    [ConfigRangeConstraint(0.5f, 2f)]
    [ConfigStepSize(0.05f)]
    [JsonInclude]
    public float NightmareSpeed = 1.4f;
    
    [ConfigMenuEntry("Hider Speed", "Speed")]
    [ConfigRangeConstraint(0.5f, 2f)]
    [ConfigStepSize(0.05f)]
    [JsonInclude]
    public float HiderSpeed = 1.45f;
    
    [ConfigMenuEntry("Flashlight Item")]
    [ConfigElementProvider(typeof(BarcodeElement))]
    [JsonInclude]
    public string LightItemCrate = string.Empty;
    
    [ConfigMenuEntry("Weapon Items")]
    [ConfigElementProvider(typeof(BarcodeListElement))]
    [JsonInclude]
    public List<string> WeaponItemCrates = new List<string>();

    [ConfigMenuEntry("Dev Tools Disabled", "Utility")] 
    [JsonInclude]
    public bool DevToolsDisabled = true;
    
    [ConfigMenuEntry("Disable Wind SFX", "Utility")]
    [JsonInclude]
    public bool DisableWindSFX = false;
    
    // For testing purposes, allows skipping the nightmare assignment and becoming a hider even if alone
    [ConfigMenuEntry("Skip Nightmare Assignment", "Utility")] 
    [JsonInclude]
    public bool SkipNightmare;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref HideDuration);
        serializer.SerializeValue(ref HuntDuration);
        serializer.SerializeValue(ref FinallyAlwaysPlays);
        serializer.SerializeValue(ref FinallyDuration);
        serializer.SerializeValue(ref TimeGainOnKill);
        serializer.SerializeValue(ref LockNightmare);
        serializer.SerializeValue(ref BlindNightmare);
        serializer.SerializeValue(ref LimitMags);
        serializer.SerializeValue(ref MagazineCapacity);
        serializer.SerializeValue(ref WeatherType);
        serializer.SerializeValue(ref SpectatorNightVision);
        serializer.SerializeValue(ref NightVision);
        serializer.SerializeValue(ref NightVisionBrightness);
        serializer.SerializeValue(ref LockHiderAvatars);
        serializer.SerializeValue(ref SlowNightmareOnDamage);
        serializer.SerializeValue(ref DropPlayer);
        serializer.SerializeValue(ref SetNightmareAvatars);
        serializer.SerializeValue(ref BalanceStats);
        serializer.SerializeValue(ref NightmareSpeed);
        serializer.SerializeValue(ref HiderSpeed);
        serializer.SerializeValue(ref LightItemCrate);
        serializer.SerializeValue(ref WeaponItemCrates);
        serializer.SerializeValue(ref DevToolsDisabled);
    }
    
    public object Clone()
    {
        return new TheHuntConfig()
        {
            HideDuration = HideDuration,
            HuntDuration = HuntDuration,
            FinallyAlwaysPlays = FinallyAlwaysPlays,
            FinallyDuration = FinallyDuration,
            TimeGainOnKill = TimeGainOnKill,
            LockNightmare = LockNightmare,
            BlindNightmare = BlindNightmare,
            LimitMags = LimitMags,
            MagazineCapacity = MagazineCapacity,
            WeatherType = WeatherType,
            NightVision = NightVision,
            SpectatorNightVision = SpectatorNightVision,
            NightVisionBrightness = NightVisionBrightness,
            LockHiderAvatars = LockHiderAvatars,
            SlowNightmareOnDamage = SlowNightmareOnDamage,
            DropPlayer = DropPlayer,
            SetNightmareAvatars = SetNightmareAvatars,
            BalanceStats = BalanceStats,
            NightmareSpeed = NightmareSpeed,
            HiderSpeed = HiderSpeed,
            LightItemCrate = LightItemCrate,
            WeaponItemCrates = new List<string>(WeaponItemCrates),
            DevToolsDisabled = DevToolsDisabled,
            DisableWindSFX = DisableWindSFX,
            SkipNightmare = SkipNightmare
        };
    }
}