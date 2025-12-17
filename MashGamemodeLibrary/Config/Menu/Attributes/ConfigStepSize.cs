namespace MashGamemodeLibrary.Config.Menu.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigStepSize : Attribute
{
    public object StepSize { get; }
    public ConfigStepSize(object stepSize)
    {
        StepSize = stepSize;
    }
}