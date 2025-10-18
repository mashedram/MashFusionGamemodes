namespace MashGamemodeLibrary.Config.Menu;

public interface IConfigDisplayTransformer
{
    public object ToDisplay(object value);
    public object FromDisplay(object display);
}

[AttributeUsage(AttributeTargets.Field)]
public class ConfigDisplayTransformer : Attribute
{
    public Type DisplayType { get; }
    public Type TransformerType { get; }
    public ConfigDisplayTransformer(Type displayType, Type transformerTypeType)
    {
        DisplayType = displayType;
        TransformerType = transformerTypeType;
    }
}