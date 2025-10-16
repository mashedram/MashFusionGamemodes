namespace MashGamemodeLibrary.Config.Constraints;

[AttributeUsage(AttributeTargets.Field)]
public class RangeConstraint : Attribute
{
    public int Lower { get; }
    public int Higher { get;  }
    public RangeConstraint(int lower, int higher)
    {
        Lower = lower;
        Higher = higher;
    }
}