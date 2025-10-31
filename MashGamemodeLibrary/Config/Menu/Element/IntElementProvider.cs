using LabFusion.Menu.Data;
using MashGamemodeLibrary.Config.Menu.Attributes;

namespace MashGamemodeLibrary.Config.Menu.Element;

public class IntElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<object> setter)
    {
        var min = (int?)entry.Bounds?.Lower ?? 1;
        var max = (int?)entry.Bounds?.Upper ?? 10;
            
        return new IntElementData
        {
            Title = entry.Name,
            Increment = (int?)entry.Increment ?? 1,
            MinValue = min,
            MaxValue = max,
            Value = (int)entry.DefaultValue,
            OnValueChanged = value => setter(value)
        };
    }
}