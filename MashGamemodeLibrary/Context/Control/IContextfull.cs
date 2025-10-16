namespace MashGamemodeLibrary.Context.Control;

public interface IContextfull<in T> where T: GameModeContext
{
    void SetContext(T context);
}