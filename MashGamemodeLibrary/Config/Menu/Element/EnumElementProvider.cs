using LabFusion.Menu.Data;
using MashGamemodeLibrary.Config.Menu.Attributes;

namespace MashGamemodeLibrary.Config.Menu.Element;

public class EnumElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter)
    {
        return new EnumElementData
        {
            Title = entry.Name,
            EnumType = entry.Type,
            Value = (Enum)entry.Value,
            OnValueChanged = value => setter(entry, value)
        };
    }
}