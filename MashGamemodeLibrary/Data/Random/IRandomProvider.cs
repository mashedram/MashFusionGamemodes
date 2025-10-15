namespace MashGamemodeLibrary.Data.Random;

public interface IRandomProvider<out TValue>
{
    public TValue? GetRandomValue();
}