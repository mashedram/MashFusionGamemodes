using Clockhunt.Audio.Effectors.Weather;
using Clockhunt.Nightmare;
using LabFusion.Extensions;
using LabFusion.Menu.Data;
using MashGamemodeLibrary.Loadout;
using MelonLoader;
using UnityEngine;

namespace Clockhunt.Config;

public static class ClockhuntConfigMenu
{
    private static int MinutesToSeconds(float minutes) => (int)(minutes * 60);
    private static float SecondsToMinutes(int seconds) => seconds / 60f;
    
    public static GroupElementData CreateSettingsGroup()
    {
        var group = new GroupElementData("Clockhunt Settings");

        group.AddElement(new FloatElementData
        {
            Title = "Hide Phase Duration (Minutes)",
            Increment = 0.25f,
            MinValue = 0.25f,
            MaxValue = 10f,
            Value = SecondsToMinutes(ClockhuntConfig.HidePhaseDuration),
            OnValueChanged = value => ClockhuntConfig.HidePhaseDuration = MinutesToSeconds(value)
        });
        
        group.AddElement(new FloatElementData
        {
            Title = "Hunt Phase Duration (Minutes)",
            Increment = 0.25f,
            MinValue = 0.25f,
            MaxValue = 30f,
            Value = SecondsToMinutes(ClockhuntConfig.HuntPhaseDuration),
            OnValueChanged = value => ClockhuntConfig.HuntPhaseDuration = MinutesToSeconds(value)
        });
        
        group.AddElement(new FloatElementData
        {
            Title = "Escape Phase Duration (Minutes)",
            Increment = 0.25f,
            MinValue = 0.25f,
            MaxValue = 10f,
            Value = SecondsToMinutes(ClockhuntConfig.EscapePhaseDuration),
            OnValueChanged = value => ClockhuntConfig.EscapePhaseDuration = MinutesToSeconds(value)
        });
        
        group.AddElement(new IntElementData
        {
            Title = "Clocks Per Player",
            Increment = 1,
            MinValue = 1,
            MaxValue = 10,
            Value = ClockhuntConfig.ClocksPerPlayer,
            OnValueChanged = value => ClockhuntConfig.ClocksPerPlayer = value
        });
        
        group.AddElement(new IntElementData
        {
            Title = "Hunt Phase Clock Count",
            Increment = 1,
            MinValue = 1,
            MaxValue = 20,
            Value = ClockhuntConfig.HuntPhaseClockCount,
            OnValueChanged = value => ClockhuntConfig.HuntPhaseClockCount = value
        });
        
        group.AddElement(new BoolElementData
        {
            Title = "Spectator Mode Enabled",
            Value = ClockhuntConfig.IsSpectatingEnabled,
            OnValueChanged = value => ClockhuntConfig.IsSpectatingEnabled = value
        });
        
        group.AddElement(new BoolElementData
        {
            Title = "Escape Phase Enabled",
            Value = ClockhuntConfig.IsEscapePhaseEnabled,
            OnValueChanged = value => ClockhuntConfig.IsEscapePhaseEnabled = value
        });
        
        group.AddElement(new BoolElementData
        {
            Title = "Teleport To Spawns on Hunt",
            Value = ClockhuntConfig.TeleportToSpawn,
            OnValueChanged = value => ClockhuntConfig.TeleportToSpawn.Value = value
        });
        
        group.AddElement(new BoolElementData
        {
            Title = "Night Vision",
            Value = ClockhuntConfig.NightVision,
            OnValueChanged = value => ClockhuntConfig.NightVision.Value = value
        });
        
        group.AddElement(new FloatElementData
        {
            Title = "Night Vision Brightness",
            Increment = 0.1f,
            MinValue = 0.1f,
            MaxValue = 2f,
            Value = ClockhuntConfig.NightVisionBrightness,
            OnValueChanged = value => ClockhuntConfig.NightVisionBrightness.Value = value
        });
        
        group.AddElement(new EnumElementData
        {
            EnumType = typeof(WeatherType),
            OnValueChanged = value => ClockhuntConfig.WeatherType.Value = (WeatherType)value,
            Title = "Weather Type",
            Value = ClockhuntConfig.WeatherType.Value
        });
        
        group.AddElement(new BoolElementData
        {
            Title = "Disable Dev Tools",
            Value = ClockhuntConfig.DevToolsDisabled,
            OnValueChanged = value => ClockhuntConfig.DevToolsDisabled = value
        });
        
        group.AddElement(new SpawnableElementData
        {
            Title = "Set Primary Weapon",
            OnSetSpawnable = value => MelonLogger.Msg($"Setting primary weapon to {value._id ?? "null"}")
        });

        var nightmares = new GroupElementData("Nightmare Toggles");
        NightmareManager.Descriptors.Values.ForEach(descriptor =>
        {
            nightmares.AddElement(new BoolElementData
            {
                Title = descriptor.Name,
                Value = descriptor.IsEnabled,
                OnValueChanged = value => descriptor.IsEnabled = value
            });
        });
        group.AddElement(nightmares);
        
        return group;
    }
}