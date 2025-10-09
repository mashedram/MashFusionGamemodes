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

    public static IEnumerable<KeyValuePair<int, T>> WithIndices<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();
        
        var index = 0;
        while (enumerator.MoveNext())
        {
            yield return new KeyValuePair<int, T>(index, enumerator.Current);
            index += 1;
        }
    }
}