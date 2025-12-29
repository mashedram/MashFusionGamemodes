namespace MashGamemodeLibrary.Execution;

public enum ExecutionContext
{
    Host,
    Remote
}

[AttributeUsage(AttributeTargets.Method)]
public class RunIf : Attribute
{
    public ExecutionContext ExecutionContext { get; }
    
    public RunIf(ExecutionContext executionContext)
    {
        ExecutionContext = executionContext;
    }
}