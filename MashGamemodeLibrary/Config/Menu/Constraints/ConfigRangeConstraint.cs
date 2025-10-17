namespace MashGamemodeLibrary.Config.Constraints;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigRangeConstraint : Attribute
{
    public object Lower { get; }
    public object Higher { get;  }
    public ConfigRangeConstraint(object lower, object higher)
    {
        Lower = lower;
        Higher = higher;
    }
}