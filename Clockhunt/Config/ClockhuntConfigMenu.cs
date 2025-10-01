using Clockhunt.Nightmare;
using LabFusion.Extensions;
using LabFusion.Menu.Data;

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
            Title = "Number of Nightmares",
            Increment = 1,
            MinValue = 1,
            MaxValue = 10,
            Value = ClockhuntConfig.NightmareCount,
            OnValueChanged = value => ClockhuntConfig.NightmareCount = value
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