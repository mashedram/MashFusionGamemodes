namespace Clockhunt.Util;

public static class EnumerableExtender
{
    public static IEnumerable<T> ExceptLast<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
            yield break;

        T previous = enumerator.Current;
        while (enumerator.MoveNext())
        {
            yield return previous;
            previous = enumerator.Current;
        }
    }
}