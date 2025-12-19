using LabFusion.Menu.Data;

namespace MashGamemodeLibrary.Config.Menu.Attributes;

public interface IConfigElementProvider
{
    public ElementData GetElementData(ConfigEntryData entry, Action<ConfigEntryData, object> setter);
}

[AttributeUsage(AttributeTargets.Field)]
public class ConfigElementProvider : Attribute
{
    public ConfigElementProvider(Type providerTypeType)
    {
        ProviderType = providerTypeType;
    }
    public Type ProviderType { get; }
}