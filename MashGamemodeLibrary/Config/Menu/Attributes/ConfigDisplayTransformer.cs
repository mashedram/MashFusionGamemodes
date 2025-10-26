using LabFusion.Menu.Data;

namespace MashGamemodeLibrary.Config.Menu.Attributes;

public interface IConfigElementProvider
{
    public ElementData GetElementData(string name, object value, Action<object> setter);
}

[AttributeUsage(AttributeTargets.Field)]
public class ConfigElementProvider : Attribute
{
    public Type ProviderType { get; }
    public ConfigElementProvider(Type providerTypeType)
    {
        ProviderType = providerTypeType;
    }
}