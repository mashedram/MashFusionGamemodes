namespace MashGamemodeLibrary.Config.Menu.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigStepSize : Attribute
{
    public ConfigStepSize(object stepSize)
    {
        StepSize = stepSize;
    }
    public object StepSize { get; }
}