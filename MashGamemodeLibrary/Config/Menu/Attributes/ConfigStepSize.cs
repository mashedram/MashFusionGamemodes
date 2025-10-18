namespace MashGamemodeLibrary.Config.Menu;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigStepSize : Attribute
{
    public object StepSize { get; }
    public ConfigStepSize(object stepSize)
    {
        StepSize = stepSize;
    }
}