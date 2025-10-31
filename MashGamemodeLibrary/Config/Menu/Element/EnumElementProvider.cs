using LabFusion.Menu.Data;
using MashGamemodeLibrary.Config.Menu.Attributes;

namespace MashGamemodeLibrary.Config.Menu.Element;

public class EnumElementProvider : IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<object> setter)
    {
        return new EnumElementData
        {
            Title = entry.Name,
            EnumType = entry.Type,
            Value = (Enum)entry.DefaultValue,
            OnValueChanged = setter
        };
    }
}