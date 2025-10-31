using LabFusion.Menu.Data;
using MashGamemodeLibrary.Config.Menu.Attributes;

namespace MashGamemodeLibrary.Config.Menu.Element;

public class BoolElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<object> setter)
    {
        return new BoolElementData
        {
            Title = entry.Name,
            Value = (bool)entry.DefaultValue,
            OnValueChanged = value => setter(value)
        };
    }
}