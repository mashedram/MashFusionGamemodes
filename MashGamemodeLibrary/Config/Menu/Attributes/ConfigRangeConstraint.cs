namespace MashGamemodeLibrary.Config.Menu.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigRangeConstraint : Attribute
{
    public ConfigRangeConstraint(object lower, object upper)
    {
        Lower = lower;
        Upper = upper;
    }
    public object Lower { get; }
    public object Upper { get; }
}