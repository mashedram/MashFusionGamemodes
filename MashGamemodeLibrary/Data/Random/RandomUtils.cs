namespace MashGamemodeLibrary.Data.Random;

public static class RandomUtils
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> from)
    {
        var rng = new System.Random();
        return from.OrderBy(_ => rng.Next());
    }
}