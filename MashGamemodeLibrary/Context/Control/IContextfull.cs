namespace MashGamemodeLibrary.Context.Control;

public interface IContextfull<in T> where T: GameModeContext<T>
{
    void SetContext(T context);
}