using LabFusion.Menu.Data;
using MashGamemodeLibrary.Config.Menu.Attributes;

namespace MashGamemodeLibrary.Config.Menu.Element;

public class FloatElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<object> setter)
    {
        var min = (float?)entry.Bounds?.Lower ?? 1f;
        var max = (float?)entry.Bounds?.Upper ?? 10f;
            
        return new FloatElementData
        {
            Title = entry.Name,
            Increment = (float?)entry.Increment ?? 1f,
            MinValue = min,
            MaxValue = max,
            Value = (float)entry.DefaultValue,
            OnValueChanged = value => setter(value)
        };
    }
}