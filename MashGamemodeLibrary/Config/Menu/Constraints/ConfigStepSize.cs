namespace MashGamemodeLibrary.Config.Menu;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigStepSize : Attribute
{
    public int StepSize { get; }
    public ConfigStepSize(int stepSize)
    {
        StepSize = stepSize;
    }
}