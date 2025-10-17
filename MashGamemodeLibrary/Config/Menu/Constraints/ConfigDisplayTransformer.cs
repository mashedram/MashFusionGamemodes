namespace MashGamemodeLibrary.Config.Menu;

public interface IConfigDisplayTransformer<TValue, TDisplay>
{
    public TDisplay ToDisplay(TValue value);
    public TValue FromDisplay(TDisplay display);
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