using Clockhunt.Audio.Effectors.Weather;
using Clockhunt.Nightmare;
using LabFusion.Extensions;
using LabFusion.Menu.Data;
using MashGamemodeLibrary.Loadout;
using MelonLoader;

namespace Clockhunt.Config;

public static class ClockhuntConfigMenu
{
    private static int MinutesToSeconds(float minutes)
    {
        return (int)(minutes * 60);
    }

    private static float SecondsToMinutes(int seconds)
    {
        return seconds / 60f;
    }

    public static GroupElementData CreateSettingsGroup()
    {
        var group = new GroupElementData("Clockhunt Settings");

        group.AddElement(new FloatElementData
        {
            Title = "Hide Phase Duration (Minutes)",
            Increment = 0.25f,
            MinValue = 0.25f,
            MaxValue = 10f,
            Value = SecondsToMinutes(Clockhunt.Config.HidePhaseDuration),
            OnValueChanged = value => Clockhunt.Config.HidePhaseDuration = MinutesToSeconds(value)
        });

        group.AddElement(new FloatElementData
        {
            Title = "Hunt Phase Duration (Minutes)",
            Increment = 0.25f,
            MinValue = 0.25f,
            MaxValue = 30f,
            Value = SecondsToMinutes(Clockhunt.Config.HuntPhaseDuration),
            OnValueChanged = value => Clockhunt.Config.HuntPhaseDuration = MinutesToSeconds(value)
        });

        group.AddElement(new FloatElementData
        {
            Title = "Escape Phase Duration (Minutes)",
            Increment = 0.25f,
            MinValue = 0.25f,
            MaxValue = 10f,
            Value = SecondsToMinutes(Clockhunt.Config.EscapePhaseDuration),
            OnValueChanged = value => Clockhunt.Config.EscapePhaseDuration = MinutesToSeconds(value)
        });

        group.AddElement(new IntElementData
        {
            Title = "Clocks Per Player",
            Increment = 1,
            MinValue = 1,
            MaxValue = 10,
            Value = Clockhunt.Config.ClocksPerPlayer,
            OnValueChanged = value => Clockhunt.Config.ClocksPerPlayer = value
        });

        group.AddElement(new IntElementData
        {
            Title = "Hunt Phase Clock Count",
            Increment = 1,
            MinValue = 1,
            MaxValue = 20,
            Value = Clockhunt.Config.HuntPhaseClockCount,
            OnValueChanged = value => Clockhunt.Config.HuntPhaseClockCount = value
        });

        group.AddElement(new BoolElementData
        {
            Title = "Spectator Mode Enabled",
            Value = Clockhunt.Config.IsSpectatingEnabled,
            OnValueChanged = value => Clockhunt.Config.IsSpectatingEnabled = value
        });

        group.AddElement(new BoolElementData
        {
            Title = "Escape Phase Enabled",
            Value = Clockhunt.Config.IsEscapePhaseEnabled,
            OnValueChanged = value => Clockhunt.Config.IsEscapePhaseEnabled = value
        });

        group.AddElement(new BoolElementData
        {
            Title = "Teleport To Spawns on Hunt",
            Value = Clockhunt.Config.TeleportToSpawn,
            OnValueChanged = value => Clockhunt.Config.TeleportToSpawn = value
        });

        group.AddElement(new BoolElementData
        {
            Title = "Night Vision",
            Value = Clockhunt.Config.NightVision,
            OnValueChanged = value => Clockhunt.Config.NightVision = value
        });

        group.AddElement(new FloatElementData
        {
            Title = "Night Vision Brightness",
            Increment = 0.1f,
            MinValue = 0.1f,
            MaxValue = 2f,
            Value = Clockhunt.Config.NightVisionBrightness,
            OnValueChanged = value => Clockhunt.Config.NightVisionBrightness = value
        });

        group.AddElement(new EnumElementData
        {
            EnumType = typeof(WeatherType),
            OnValueChanged = value => Clockhunt.Config.WeatherType = (WeatherType)value,
            Title = "Weather Type",
            Value = Clockhunt.Config.WeatherType
        });

        group.AddElement(new BoolElementData
        {
            Title = "Runtime Spawn Points",
            Value = Clockhunt.Config.RuntimeSpawnPointsEnabled,
            OnValueChanged = value => Clockhunt.Config.RuntimeSpawnPointsEnabled = value
        });

        group.AddElement(new IntElementData
        {
            Title = "Runtime Spawn Point Count",
            Value = Clockhunt.Config.RuntimeSpawnCount,
            Increment = 1,
            MaxValue = 20,
            MinValue = 0,
            OnValueChanged = value => Clockhunt.Config.RuntimeSpawnCount = value
        });

        group.AddElement(new BoolElementData
        {
            Title = "Disable Dev Tools",
            Value = Clockhunt.Config.DevToolsDisabled,
            OnValueChanged = value => Clockhunt.Config.DevToolsDisabled = value
        });

#if DEBUG
        group.AddElement(new BoolElementData
        {
            Title = "Force spectator on final death",
            Value = Clockhunt.Config.DebugForceSpectate,
            OnValueChanged = value => Clockhunt.Config.DebugForceSpectate = value
        });
        
        group.AddElement(new BoolElementData
        {
            Title = "Skip nightmare assignment",
            Value = Clockhunt.Config.DebugSkipNightmare,
            OnValueChanged = value => Clockhunt.Config.DebugSkipNightmare = value
        });
#endif

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
                OnValueChanged = value =>
                {
                    if (NightmareManager.Descriptors.Count(v => v.Value.IsEnabled) <= 1)
                        return;

                    descriptor.IsEnabled = value;
                }
            });
        });
        group.AddElement(nightmares);

        return group;
    }
}