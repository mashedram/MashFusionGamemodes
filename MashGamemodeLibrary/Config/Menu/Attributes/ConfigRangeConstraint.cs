namespace MashGamemodeLibrary.Config.Constraints;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigRangeConstraint : Attribute
{
    public object Lower { get; }
    public object Upper { get; }
    public ConfigRangeConstraint(object lower, object upper)
    {
        Lower = lower;
        Upper = upper;
    }
}