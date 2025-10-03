namespace MashGamemodeLibrary.Environment;

public abstract class EnvironmentEffector<TContext>
{
    public virtual bool CanApply(TContext context)
    {
        return true;
    }
    
    public virtual void Apply(TContext context) {}
    public virtual void Update(TContext context, float delta) {}
    public virtual void Remove(TContext context) {}
}