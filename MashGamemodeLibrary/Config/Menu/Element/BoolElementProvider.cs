using LabFusion.Menu.Data;
using MashGamemodeLibrary.Config.Menu.Attributes;

namespace MashGamemodeLibrary.Config.Menu.Element;

public class BoolElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter)
    {
        return new BoolElementData
        {
            Title = entry.Name,
            Value = (bool)entry.Value,
            OnValueChanged = value => setter(entry, value)
        };
    }
}